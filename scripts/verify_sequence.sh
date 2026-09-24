#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
CHECK_STAMP="$(date -u +%Y%m%d_%H%M%S)"
export CHUPA_EXCHANGE_EVIDENCE="docs/evidencias/exchange_${CHECK_STAMP}"
export CHUPA_BLOCKING_EVIDENCE="docs/evidencias/blocking_${CHECK_STAMP}"
mkdir -p "$CHUPA_EXCHANGE_EVIDENCE" "$CHUPA_BLOCKING_EVIDENCE"
# GPU is required by the rendered masks and UV checks; no -nographics here.
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod ExchangeBuild.Verify -logFile "$CHUPA_EXCHANGE_EVIDENCE/numeric.log"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod ExchangeBuild.VerifyRuntimeAndWindow -logFile "$CHUPA_EXCHANGE_EVIDENCE/runtime.log"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod BlockingBuild.VerifyAndCapture -logFile "$CHUPA_BLOCKING_EVIDENCE/visual.log"
