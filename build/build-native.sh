#!/usr/bin/env bash
# Build the native HeadsetControl shared library and place it under
# build/native/<rid>/ where the .NET projects pick it up.
#
#   build/build-native.sh                 host RID
#   build/build-native.sh --rid <rid>     osx-arm64, osx-x64, linux-x64, linux-arm64, win-x64
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

# Pin the macOS architecture so the produced dylib matches the target RID
# even when CMake/CMake's host detection would otherwise pick a different
# arch (e.g. running under Rosetta).
case "${RID}" in
    osx-arm64) RID_CMAKE_ARGS="-DCMAKE_OSX_ARCHITECTURES=arm64"  ;;
    osx-x64)   RID_CMAKE_ARGS="-DCMAKE_OSX_ARCHITECTURES=x86_64" ;;
    *)         RID_CMAKE_ARGS=""                                  ;;
esac

echo ">> Configuring native HeadsetControl for ${RID}"
# shellcheck disable=SC2086
cmake -S "${NATIVE_SRC}" -B "${BUILD_DIR}" \
    -DBUILD_SHARED_LIBRARY=ON \
    -DCMAKE_BUILD_TYPE=Release \
    ${RID_CMAKE_ARGS} \
    ${CMAKE_EXTRA_ARGS:-}

echo ">> Building"
cmake --build "${BUILD_DIR}" --config Release --target headsetcontrol_shared

case "${RID}" in
    osx-*)
        cp "${BUILD_DIR}/libheadsetcontrol.dylib" "${OUT_DIR}/libheadsetcontrol.dylib"

        # Bundle the hidapi dylib next to libheadsetcontrol and rewrite the
        # absolute install_name reference to a relative @loader_path lookup,
        # so consumers don't need brew install hidapi at runtime.
        for DEP in $(otool -L "${OUT_DIR}/libheadsetcontrol.dylib" | awk 'NR>1 {print $1}' | grep -E 'libhidapi[^/]*\.dylib$'); do
            DEP_NAME=$(basename "${DEP}")
            if [[ -f "${DEP}" ]]; then
                cp -L "${DEP}" "${OUT_DIR}/${DEP_NAME}"
                chmod u+w "${OUT_DIR}/${DEP_NAME}"
                install_name_tool -change "${DEP}" "@loader_path/${DEP_NAME}" \
                    "${OUT_DIR}/libheadsetcontrol.dylib"
                install_name_tool -id "@loader_path/${DEP_NAME}" "${OUT_DIR}/${DEP_NAME}" 2>/dev/null || true
                # install_name_tool invalidates the existing code signature;
                # re-sign with an ad-hoc identity so dyld accepts the lib on
                # hardened-runtime hosts.
                codesign --force --sign - "${OUT_DIR}/${DEP_NAME}" 2>/dev/null || true
                echo ">> Bundled ${DEP_NAME} from ${DEP}"
            else
                echo "error: hidapi dependency ${DEP} not found, package would not be self-contained" >&2
                exit 1
            fi
        done
        codesign --force --sign - "${OUT_DIR}/libheadsetcontrol.dylib" 2>/dev/null || true

        # Fail the build if any absolute path remains in the dependency list.
        REMAINING=$(otool -L "${OUT_DIR}/libheadsetcontrol.dylib" | awk 'NR>1 {print $1}' | grep -E '^/(opt|usr/local|Users)' || true)
        if [[ -n "${REMAINING}" ]]; then
            echo "error: libheadsetcontrol.dylib still references absolute paths:" >&2
            echo "${REMAINING}" >&2
            exit 1
        fi
        ;;
    linux-*)
        # Skip libheadsetcontrol.so.X.Y.Z; copy the SONAME symlink target.
        SRC=$(ls "${BUILD_DIR}/libheadsetcontrol.so"* 2>/dev/null | grep -v '\.so\.[0-9]\+\.[0-9]\+\.[0-9]\+' | head -n1)
        [[ -z "${SRC}" ]] && SRC="${BUILD_DIR}/libheadsetcontrol.so"
        cp "${SRC}" "${OUT_DIR}/libheadsetcontrol.so"

        # Bundle the hidapi .so and set RPATH=$ORIGIN so the loader picks the
        # adjacent copy instead of relying on a system install.
        if ! command -v patchelf >/dev/null 2>&1; then
            echo "error: patchelf required to make the package self-contained on Linux" >&2
            exit 1
        fi

        for DEP_NAME in $(patchelf --print-needed "${OUT_DIR}/libheadsetcontrol.so" | grep -E '^libhidapi'); do
            DEP_PATH=$(ldconfig -p | awk -v n="${DEP_NAME}" '$1 == n {print $NF; exit}')
            if [[ -n "${DEP_PATH}" && -f "${DEP_PATH}" ]]; then
                cp -L "${DEP_PATH}" "${OUT_DIR}/${DEP_NAME}"
                echo ">> Bundled ${DEP_NAME} from ${DEP_PATH}"
            else
                echo "error: ${DEP_NAME} not found via ldconfig" >&2
                exit 1
            fi
        done
        patchelf --set-rpath '$ORIGIN' "${OUT_DIR}/libheadsetcontrol.so"
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
