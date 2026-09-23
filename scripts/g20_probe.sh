#!/usr/bin/env bash
# Run by the agent once M#[2] supplies physical USB authorization. No phone setup is automated.
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
ADB='/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb'
PACKAGE='com.chupacabras.ar.trackingprobe'
MODE="${1:-inspect}"
case "$MODE" in inspect|install|collect) ;; *) echo 'Modo: inspect | install ruta.apk | collect' >&2; exit 2;; esac
cd "$PROJECT_ROOT"
# Avoid selecting a different attached phone by accident.
mapfile -t DEVICES < <("$ADB" devices | awk 'NR>1 && $2=="device" {print $1}')
if [[ ${#DEVICES[@]} != 1 ]]; then
  "$ADB" devices -l
  echo 'Hace falta exactamente un dispositivo autorizado; no se instaló nada.' >&2; exit 3
fi
SERIAL="${DEVICES[0]}"
MODEL="$("$ADB" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
if [[ ! "${MODEL,,}" =~ moto.*g.?20 ]]; then echo "Dispositivo inesperado: $MODEL; detener y comprobar." >&2; exit 4; fi
ABI_LIST="$("$ADB" -s "$SERIAL" shell getprop ro.product.cpu.abilist | tr -d '\r')"
SDK_LEVEL="$("$ADB" -s "$SERIAL" shell getprop ro.build.version.sdk | tr -d '\r')"
STAMP="$(date -u +%Y%m%d_%H%M%S)"
OUTPUT="docs/evidencias/g20_${MODE}_${STAMP}"
mkdir "$OUTPUT"
for PROPERTY in ro.product.model ro.product.manufacturer ro.build.version.release ro.build.version.sdk ro.product.cpu.abilist ro.product.cpu.abilist64; do
  printf '%s=' "$PROPERTY" >> "$OUTPUT/device.txt"
  "$ADB" -s "$SERIAL" shell getprop "$PROPERTY" >> "$OUTPUT/device.txt"
done
"$ADB" -s "$SERIAL" shell getconf PAGE_SIZE >> "$OUTPUT/device.txt"
if [[ ",$ABI_LIST," != *,arm64-v8a,* ]]; then
  echo "Sin ABI arm64-v8a: $ABI_LIST. Conservar diagnóstico y detener; no instalar." >&2; exit 5
fi
if [[ ! "$SDK_LEVEL" =~ ^[0-9]+$ ]] || (( SDK_LEVEL < 26 )); then
  echo "Android API incompatible: $SDK_LEVEL; no instalar." >&2; exit 6
fi
if [[ "$MODE" == install ]]; then
  APK="${2:?Hace falta la ruta de la APK verificada}"
  [[ -f "$APK" ]]
  # -r preserves the probe's data if a later revision is installed. Never uninstall.
  "$ADB" -s "$SERIAL" install -r "$APK" | tee "$OUTPUT/install.txt"
  "$ADB" -s "$SERIAL" shell am start -W -n "$PACKAGE/com.unity3d.player.UnityPlayerGameActivity" > "$OUTPUT/launch.txt"
elif [[ "$MODE" == collect ]]; then
  if ! APP_PID="$("$ADB" -s "$SERIAL" shell pidof "$PACKAGE" | tr -d '\r')"; then
    APP_PID=""
  fi
  if [[ "$APP_PID" =~ ^[0-9]+$ ]]; then
    "$ADB" -s "$SERIAL" logcat -d --pid="$APP_PID" -v threadtime > "$OUTPUT/logcat.txt"
  else
    echo 'La app no está ejecutándose; no se recopilan logs de otros procesos.' > "$OUTPUT/logcat.txt"
  fi
  "$ADB" -s "$SERIAL" shell dumpsys meminfo "$PACKAGE" > "$OUTPUT/memory.txt"
  FOREGROUND="$("$ADB" -s "$SERIAL" shell dumpsys activity activities | awk '/mResumedActivity:/{print; exit}')"
  if [[ "$FOREGROUND" == *"$PACKAGE/"* ]]; then
    "$ADB" -s "$SERIAL" exec-out screencap -p > "$OUTPUT/screen.png"
  fi
  mkdir "$OUTPUT/files"
  while IFS= read -r PROBE_DIR; do
    PROBE_DIR="${PROBE_DIR//$'\r'/}"
    if [[ "$PROBE_DIR" =~ ^probe_[0-9_]+$ ]]; then
      "$ADB" -s "$SERIAL" pull "/sdcard/Android/data/$PACKAGE/files/$PROBE_DIR" "$OUTPUT/files/$PROBE_DIR"
    fi
  done < <("$ADB" -s "$SERIAL" shell ls -1 "/sdcard/Android/data/$PACKAGE/files")
fi
printf 'Resultado: %s\n' "$OUTPUT"
