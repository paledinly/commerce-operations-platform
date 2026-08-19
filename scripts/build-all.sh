#!/usr/bin/env sh
. "$(dirname "$0")/common.sh"; cd "$ROOT/commerce-operations-dotnet"; run_step CSharp Build dotnet build; cd "$ROOT/commerce-order-engine-java"; run_step Java Build ./gradlew clean build; cd "$ROOT/commerce-operations-react"; run_step React Install npm install; run_step React Build npm run build
