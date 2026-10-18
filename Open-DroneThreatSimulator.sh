#!/bin/sh

# Open this Unity project through Unity Hub. The path is derived from this
# script, so it also works after the project folder is moved.
PROJECT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)

if ! command -v unityhub >/dev/null 2>&1; then
    printf '%s\n' "Unity Hub was not found. Install Unity Hub, then run this launcher again." >&2
    exit 1
fi

exec unityhub -- --projectPath "$PROJECT_DIR"
