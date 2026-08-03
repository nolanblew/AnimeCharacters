#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Register each extension updater here so local and CI refreshes stay aligned.
updaters=(
  "$SCRIPT_DIR/update-genshin-impact-data.sh"
)

for updater in "${updaters[@]}"; do
  "$updater" "$@"
done
