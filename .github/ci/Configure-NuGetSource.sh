#!/usr/bin/env bash
set -euo pipefail

: "${GITHUB_TOKEN:?GITHUB_TOKEN must be set}"
: "${GITHUB_ENV:?GITHUB_ENV must be set}"

echo "::add-mask::${GITHUB_TOKEN}"
echo "NOP77SVK.GITHUB_PKG_TOKEN=${GITHUB_TOKEN}" >> "${GITHUB_ENV}"
