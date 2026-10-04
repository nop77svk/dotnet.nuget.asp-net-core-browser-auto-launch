#!/usr/bin/env bash
set -euo pipefail

git fetch --no-tags origin main

if ! git merge-base --is-ancestor "${GITHUB_SHA}" origin/main; then
	echo "::error::Tagged commit ${GITHUB_SHA} is not on main"
	exit 1
fi
