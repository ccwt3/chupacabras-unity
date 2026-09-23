#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
mkdir -p docs/evidencias
BUILD_STAMP="$(date -u +%Y%m%d_%H%M%S)"
export DOTNET_PROCESSOR_COUNT=2
export GRADLE_USER_HOME="$PROJECT_ROOT/Library/GradleUserHome"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod TrackingBuild.ConfigureAndVerify -logFile "docs/evidencias/tracking_configure_${BUILD_STAMP}.log"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -buildTarget Android \
  -executeMethod TrackingBuild.BuildAndroid -logFile "docs/evidencias/tracking_build_${BUILD_STAMP}.log"
