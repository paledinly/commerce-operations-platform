#!/usr/bin/env sh
. "$(dirname "$0")/common.sh"; require_env; cd "$ROOT"; docker compose -f "$COMPOSE" --env-file "$ENV_FILE" down
