#!/usr/bin/env sh
. "$(dirname "$0")/common.sh"; "$ROOT/scripts/build-all.sh"; "$ROOT/scripts/test-all.sh"; cd "$ROOT/commerce-operations-react"; run_step React Lint npm run lint; require_env; cd "$ROOT"; run_step Docker Config docker compose -f "$COMPOSE" --env-file "$ENV_FILE" config; run_step Docker Images docker compose -f "$COMPOSE" --env-file "$ENV_FILE" build
