#!/usr/bin/env sh
. "$(dirname "$0")/common.sh"; cd "$ROOT/commerce-operations-dotnet"; run_step CSharp Test dotnet test; cd "$ROOT/commerce-order-engine-java"; run_step Java Test ./gradlew test; cd "$ROOT/commerce-operations-react"; run_step React Test npm run test
