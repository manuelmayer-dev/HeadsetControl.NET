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
cmake -S "${NATIVE_SRC}" -B "${BUILD_DIR}" \
    -DBUILD_SHARED_LIBRARY=ON \
    -DCMAKE_BUILD_TYPE=Release

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
        cp "${BUILD_DIR}/headsetcontrol.dll" "${OUT_DIR}/headsetcontrol.dll"
        ;;
    *)
        echo "error: don't know how to copy artefacts for ${RID}" >&2
        exit 1
        ;;
esac

echo ">> Installed: ${OUT_DIR}/"
ls -la "${OUT_DIR}"
