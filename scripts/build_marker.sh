#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_ROOT"
MARKER_OUTPUT="${1:-marker/rebuild_$(date -u +%Y%m%d_%H%M%S)}"
if [[ -e "$MARKER_OUTPUT" ]]; then echo "La salida ya existe: $MARKER_OUTPUT" >&2; exit 1; fi
mkdir -p "$MARKER_OUTPUT"
"${TYPST_BIN:-typst}" compile --package-cache-path "$PROJECT_ROOT/Library/TypstPackages" \
  marker/03_marker_a4.typ "$MARKER_OUTPUT/03_marker_a4.pdf"
pdftoppm -scale-to 1200 -png -singlefile "$MARKER_OUTPUT/03_marker_a4.pdf" "$MARKER_OUTPUT/03_marker_a4_preview"
sha256sum "$MARKER_OUTPUT"/* > "$MARKER_OUTPUT/SHA256SUMS"
