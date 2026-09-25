#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
RIG_STAMP="$(date -u +%Y%m%d_%H%M%S)"
for CHUPA_RIG in 10_rig_oveja 11_rig_chupacabras_poses 11_rigs_contacto_r03; do
  export CHUPA_RIG
  export CHUPA_RIG_EVIDENCE="docs/evidencias/rigs_${RIG_STAMP}/${CHUPA_RIG}"
  mkdir -p "$CHUPA_RIG_EVIDENCE"
  "$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
    -executeMethod RigBuild.Verify -logFile "$CHUPA_RIG_EVIDENCE/verify.log"
done
