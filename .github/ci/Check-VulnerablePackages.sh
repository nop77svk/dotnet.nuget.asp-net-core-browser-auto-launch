#!/usr/bin/env bash
set -euo pipefail

report="$(dotnet list package --no-restore --vulnerable --include-transitive)"
echo "${report}"

if grep -q "has the following vulnerable packages" <<< "${report}"; then
	echo "::error::Vulnerable packages detected"
	exit 1
fi
