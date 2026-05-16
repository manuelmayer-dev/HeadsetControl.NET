#!/usr/bin/env bash
# Build the native HeadsetControl shared library and place it under
# build/native/<rid>/ where the .NET projects pick it up.
#
#   build/build-native.sh                 host RID
#   build/build-native.sh --rid <rid>     osx-arm64, linux-x64, linux-arm64, win-x64
#
# CMAKE_EXTRA_ARGS is forwarded to the configure step (used by CI to pass the
# vcpkg toolchain file on Windows).

set -euo pipefail

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REPO_ROOT="$( cd "${SCRIPT_DIR}/.." && pwd )"
NATIVE_SRC="${REPO_ROOT}/headsetcontrollib"
BUILD_ROOT="${SCRIPT_DIR}/cmake"
OUT_ROOT="${SCRIPT_DIR}/native"

detect_rid() {
    local os arch
    case "$(uname -s)" in
        Darwin) os="osx" ;;
        Linux)  os="linux" ;;
        MINGW*|MSYS*|CYGWIN*) os="win" ;;
        *) echo "error: unsupported OS $(uname -s)" >&2; exit 1 ;;
    esac
    case "$(uname -m)" in
        x86_64|amd64) arch="x64" ;;
        arm64|aarch64) arch="arm64" ;;
        *) echo "error: unsupported architecture $(uname -m)" >&2; exit 1 ;;
    esac
    echo "${os}-${arch}"
}

RID="$(detect_rid)"
while [[ $# -gt 0 ]]; do
    case "$1" in
        --rid) RID="$2"; shift 2 ;;
        *) echo "error: unknown option $1" >&2; exit 1 ;;
    esac
done

BUILD_DIR="${BUILD_ROOT}/${RID}"
OUT_DIR="${OUT_ROOT}/${RID}"
mkdir -p "${BUILD_DIR}" "${OUT_DIR}"

# Serialise concurrent invocations for the same RID — when this script is
# wired into a multi-targeted .NET build, MSBuild may run it in parallel for
# each TFM, and the inner CMake build directory is not concurrency-safe.
# `mkdir` is atomic across processes, so we use a sentinel directory as the
# lock. The first process builds; later ones wait, then exit early once the
# artefact is in place.
LOCK_DIR="${BUILD_DIR}.lock"
trap 'rmdir "${LOCK_DIR}" 2>/dev/null || true' EXIT

WAITED=0
while ! mkdir "${LOCK_DIR}" 2>/dev/null; do
    if [[ ${WAITED} -ge 600 ]]; then
        echo "error: timed out waiting for ${LOCK_DIR}" >&2
        exit 1
    fi
    sleep 1
    WAITED=$((WAITED + 1))
done

case "${RID}" in
    osx-*)  EXPECTED="${OUT_DIR}/libheadsetcontrol.dylib" ;;
    linux-*) EXPECTED="${OUT_DIR}/libheadsetcontrol.so"   ;;
    win-*)  EXPECTED="${OUT_DIR}/headsetcontrol.dll"      ;;
    *)      EXPECTED=""                                    ;;
esac

if [[ -n "${EXPECTED}" && -f "${EXPECTED}" ]]; then
    echo ">> ${EXPECTED} already present, skipping build"
    exit 0
fi

echo ">> Configuring native HeadsetControl for ${RID}"
# shellcheck disable=SC2086
cmake -S "${NATIVE_SRC}" -B "${BUILD_DIR}" \
    -DBUILD_SHARED_LIBRARY=ON \
    -DCMAKE_BUILD_TYPE=Release \
    ${CMAKE_EXTRA_ARGS:-}

echo ">> Building"
cmake --build "${BUILD_DIR}" --config Release --target headsetcontrol_shared

case "${RID}" in
    osx-*)
        cp "${BUILD_DIR}/libheadsetcontrol.dylib" "${OUT_DIR}/libheadsetcontrol.dylib"
        ;;
    linux-*)
        # Skip libheadsetcontrol.so.X.Y.Z; copy the SONAME symlink target.
        SRC=$(ls "${BUILD_DIR}/libheadsetcontrol.so"* 2>/dev/null | grep -v '\.so\.[0-9]\+\.[0-9]\+\.[0-9]\+' | head -n1)
        [[ -z "${SRC}" ]] && SRC="${BUILD_DIR}/libheadsetcontrol.so"
        cp "${SRC}" "${OUT_DIR}/libheadsetcontrol.so"
        ;;
    win-*)
        # MSVC multi-config puts binaries in Release/; Ninja/Make do not.
        SRC=$(find "${BUILD_DIR}" -maxdepth 3 -name "headsetcontrol.dll" \
              -not -path "*/vcpkg_installed/*" 2>/dev/null | head -n1)
        if [[ -z "${SRC}" ]]; then
            echo "error: built headsetcontrol.dll not found under ${BUILD_DIR}" >&2
            exit 1
        fi
        cp "${SRC}" "${OUT_DIR}/headsetcontrol.dll"

        HIDAPI=$(find "${BUILD_DIR}/vcpkg_installed" -name "hidapi.dll" 2>/dev/null | head -n1)
        if [[ -n "${HIDAPI}" ]]; then
            cp "${HIDAPI}" "${OUT_DIR}/hidapi.dll"
            echo ">> Bundled hidapi.dll from ${HIDAPI}"
        fi
        ;;
    *)
        echo "error: don't know how to copy artefacts for ${RID}" >&2
        exit 1
        ;;
esac

echo ">> Installed: ${OUT_DIR}/"
ls -la "${OUT_DIR}"
