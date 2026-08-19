#!/usr/bin/env sh
set -eu; ROOT=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd); docker run --rm -v "$ROOT/commerce-order-engine-java:/workspace" -w /workspace gradle:8.14-jdk21 gradle wrapper --gradle-version 8.14
