#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
mkdir -p docs/evidencias
BUILD_STAMP="$(date -u +%Y%m%d_%H%M%S)"
# Limit IL2CPP managed parallelism to avoid OOM; do not restrict native compiler CPUs.
export DOTNET_PROCESSOR_COUNT=2
export GRADLE_USER_HOME="$PROJECT_ROOT/Library/GradleUserHome"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod ProjectBuild.ConfigureAndVerify -logFile "docs/evidencias/configure_${BUILD_STAMP}.log"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -buildTarget Android \
  -executeMethod ProjectBuild.BuildAndroid -logFile "docs/evidencias/build_${BUILD_STAMP}.log"
