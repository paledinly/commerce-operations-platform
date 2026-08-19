#!/usr/bin/env sh
set -eu
ROOT=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
COMPOSE="$ROOT/commerce-platform-infra/compose.yml"; ENV_FILE="$ROOT/.env.local"
require_env() { [ -f "$ENV_FILE" ] || { echo "Missing $ENV_FILE. Copy .env.local.example to .env.local first." >&2; exit 1; }; }
run_step() { project=$1; stage=$2; shift 2; echo "[$project] $stage"; echo "> $*"; "$@" || { code=$?; echo "Project=$project; Stage=$stage; Command=$*; ExitCode=$code" >&2; exit "$code"; }; }

