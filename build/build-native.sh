#!/usr/bin/env bash
# -----------------------------------------------------------------------------
# Builds the native HeadsetControl shared library for the host platform and
# copies it into build/native/<rid>/ where the .NET projects pick it up.
#
# Usage:
#   build/build-native.sh                # build for host RID
#   build/build-native.sh --rid <rid>    # explicit RID (osx-arm64, linux-x64,
#                                        # linux-arm64, win-x64)
# -----------------------------------------------------------------------------
set -euo pipefail

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REPO_ROOT="$( cd "${SCRIPT_DIR}/.." && pwd )"
NATIVE_SRC="${REPO_ROOT}/headsetcontrollib"
BUILD_ROOT="${SCRIPT_DIR}/cmake"
OUT_ROOT="${SCRIPT_DIR}/native"

# --- detect host RID ---------------------------------------------------------
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

echo ">> Configuring native HeadsetControl for ${RID}"
# CMAKE_EXTRA_ARGS lets CI pass additional flags (e.g. the vcpkg toolchain
# file on Windows) without modifying this script.
# shellcheck disable=SC2086
cmake -S "${NATIVE_SRC}" -B "${BUILD_DIR}" \
    -DBUILD_SHARED_LIBRARY=ON \
    -DCMAKE_BUILD_TYPE=Release \
    ${CMAKE_EXTRA_ARGS:-}

echo ">> Building"
cmake --build "${BUILD_DIR}" --config Release --target headsetcontrol_shared

# --- copy artefact -----------------------------------------------------------
case "${RID}" in
    osx-*)
        cp "${BUILD_DIR}/libheadsetcontrol.dylib" "${OUT_DIR}/libheadsetcontrol.dylib"
        ;;
    linux-*)
        # CMake may produce versioned filenames (e.g. libheadsetcontrol.so.1).
        # Copy the unversioned SONAME entry the .NET project expects.
        SRC=$(ls "${BUILD_DIR}/libheadsetcontrol.so"* 2>/dev/null | grep -v '\.so\.[0-9]\+\.[0-9]\+\.[0-9]\+' | head -n1)
        [[ -z "${SRC}" ]] && SRC="${BUILD_DIR}/libheadsetcontrol.so"
        cp "${SRC}" "${OUT_DIR}/libheadsetcontrol.so"
        ;;
    win-*)
        # MSVC's default generator is multi-config and places binaries in
        # <BUILD_DIR>/Release/. Single-config generators (Ninja, Makefiles)
        # put them in <BUILD_DIR>/. Locate the artefact either way and
        # exclude vcpkg's installed copies so we always pick our own build.
        SRC=$(find "${BUILD_DIR}" -maxdepth 3 -name "headsetcontrol.dll" \
              -not -path "*/vcpkg_installed/*" 2>/dev/null | head -n1)
        if [[ -z "${SRC}" ]]; then
            echo "error: built headsetcontrol.dll not found under ${BUILD_DIR}" >&2
            exit 1
        fi
        cp "${SRC}" "${OUT_DIR}/headsetcontrol.dll"

        # Bundle the hidapi runtime DLL from vcpkg's installed tree so that
        # consumers can load the library without a separate hidapi install.
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
