#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
CHECK_STAMP="$(date -u +%Y%m%d_%H%M%S)"
for CHUPA_ACTING in 13_calma_r07 14_ataque_r07; do
  export CHUPA_ACTING
  export CHUPA_ACTING_EVIDENCE="docs/evidencias/actuacion/${CHUPA_ACTING}_${CHECK_STAMP}"
  mkdir -p "$CHUPA_ACTING_EVIDENCE"
  "$UNITY_EDITOR" -batchmode -quit -job-worker-count 2 -projectPath "$PROJECT_ROOT" \
    -executeMethod ActingBuild.Verify -logFile "$CHUPA_ACTING_EVIDENCE/verify.log"
done
