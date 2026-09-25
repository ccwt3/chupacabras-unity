#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
export CHUPA_CONTEXT_SUFFIX=_contexto_r02
CHECK_DIR="$(mktemp -d "$PROJECT_ROOT/docs/evidencias/creature_$(date -u +%Y%m%d_%H%M%S)_XXXXXX")"
for CHUPA_MODEL in 08_chupacabras_forma_r03 09_chupacabras_acabado; do
    export CHUPA_MODEL
    export CHUPA_CREATURE_EVIDENCE="$CHECK_DIR/$CHUPA_MODEL"
    mkdir "$CHUPA_CREATURE_EVIDENCE"
    "$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
        -executeMethod CreatureBuild.Verify -logFile "$CHUPA_CREATURE_EVIDENCE/verify.log"
done
