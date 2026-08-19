#!/usr/bin/env sh
. "$(dirname "$0")/common.sh"
require_env
api_port=$(sed -n 's/^DOTNET_API_PORT=//p' "$ENV_FILE"); api_port=${api_port:-5000}
java_port=$(sed -n 's/^JAVA_API_PORT=//p' "$ENV_FILE"); java_port=${java_port:-8080}
ui_port=$(sed -n 's/^REACT_PORT=//p' "$ENV_FILE"); ui_port=${ui_port:-3000}
run_step Platform "C# Liveness" curl --fail --silent "http://localhost:$api_port/health/live"
run_step Platform "C# Readiness" curl --fail --silent "http://localhost:$api_port/health/ready"
run_step Platform "Java Readiness" curl --fail --silent "http://localhost:$java_port/actuator/health"
run_step Platform "React Health" curl --fail --silent "http://localhost:$ui_port/health"
cd "$ROOT"
attempt=0
while [ "$attempt" -lt 30 ]; do
  unhealthy=$(docker compose -f "$COMPOSE" --env-file "$ENV_FILE" ps --format json | grep -v '"Health":"healthy"' || true)
  [ -z "$unhealthy" ] && break
  attempt=$((attempt + 1)); sleep 2
done
[ -z "$unhealthy" ] || { echo "Container health did not become ready within 60 seconds." >&2; exit 1; }
run_step Platform "Container Health" docker compose -f "$COMPOSE" --env-file "$ENV_FILE" ps
