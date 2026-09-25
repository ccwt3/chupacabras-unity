#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
CHECK_STAMP="$(date -u +%Y%m%d_%H%M%S)"
export CHUPA_ENV_EVIDENCE="docs/evidencias/environment_${CHECK_STAMP}"
export CHUPA_SHEEP_EVIDENCE="docs/evidencias/sheep_${CHECK_STAMP}"
mkdir -p "$CHUPA_ENV_EVIDENCE" "$CHUPA_SHEEP_EVIDENCE"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod EnvironmentBuild.Verify -logFile "$CHUPA_ENV_EVIDENCE/verify.log"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod SheepBuild.Verify -logFile "$CHUPA_SHEEP_EVIDENCE/verify.log"
