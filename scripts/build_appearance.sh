#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity'
cd "$PROJECT_ROOT"
export CHUPA_APPEARANCE_EVIDENCE="${CHUPA_APPEARANCE_EVIDENCE:-docs/evidencias/appearance_$(date -u +%Y%m%d_%H%M%S)}"
mkdir "$CHUPA_APPEARANCE_EVIDENCE"
export DOTNET_PROCESSOR_COUNT=2
export GRADLE_USER_HOME="$PROJECT_ROOT/Library/GradleUserHome"
if [[ -n "${CHUPA_APPEARANCE_SCENE:-}" && ! -f "$CHUPA_APPEARANCE_SCENE" ]]; then
  echo "Prepare the selected appearance scene before building." >&2
  exit 1
fi
if [[ -z "${CHUPA_APPEARANCE_SCENE:-}" && ! -f Assets/Scenes/12_AppearanceAR_r05.unity ]]; then
  "$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
    -executeMethod AppearanceBuild.CreateTableRevision -logFile "$CHUPA_APPEARANCE_EVIDENCE/configure.log"
fi
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -quit -projectPath "$PROJECT_ROOT" \
  -executeMethod AppearanceBuild.Verify -logFile "$CHUPA_APPEARANCE_EVIDENCE/verify.log"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -projectPath "$PROJECT_ROOT" \
  -executeMethod AppearancePreview.Capture -logFile "$CHUPA_APPEARANCE_EVIDENCE/runtime.log"
"$UNITY_EDITOR" -job-worker-count 2 -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -buildTarget Android \
  -executeMethod AppearanceBuild.BuildAndroid -logFile "$CHUPA_APPEARANCE_EVIDENCE/build.log"
