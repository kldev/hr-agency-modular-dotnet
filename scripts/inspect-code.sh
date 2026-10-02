#!/usr/bin/env bash
# JetBrains inspection at SUGGESTION, the same engine as Rider. Exits non-zero on any finding, so
# the pre-push hook and anyone running it by hand see the same verdict.
#   scripts/inspect-code.sh             build, then inspect
#   scripts/inspect-code.sh --no-build  inspect what is already built
set -euo pipefail
cd "$(dirname "$0")/.."

if ! command -v jb >/dev/null 2>&1; then
    echo "jb not found: dotnet tool install -g JetBrains.ReSharper.GlobalTools" >&2
    exit 2
fi

if [[ "${1:-}" != "--no-build" ]]; then
    dotnet build HrAgencySystem.slnx -v q -nologo
fi

report="$(mktemp --suffix=.txt)"
trap 'rm -f "$report"' EXIT

jb inspectcode HrAgencySystem.slnx --no-build --format=Text --severity=SUGGESTION \
    --output="$report" >/dev/null

# The text report lists the solution and project headers even when clean; findings are the
# indented lines that carry a file path.
findings="$(grep -cE '^ {6}\S' "$report" || true)"
if [[ "$findings" -gt 0 ]]; then
    cat "$report"
    echo "inspection: $findings finding(s)" >&2
    exit 1
fi

echo "inspection: clean"
