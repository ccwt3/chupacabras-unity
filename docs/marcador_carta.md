# Adaptación a carta e impresión — 23 de septiembre de 2026

El usuario solo tiene hojas carta. A4 (210 × 297 mm) y carta (215.9 × 279.4 mm) son tamaños distintos. Se creó `marker/03_marker_carta.pdf` con CeTZ, una página de 612 × 792 puntos, conservando la matriz AprilTag y sus dimensiones físicas. Solo cambian papel y separaciones del texto; no requiere cambio de APK. La entrega A4 anterior permanece idéntica por hash.

`build_marker.sh` genera carta por defecto. `MarkerChecks.Run` ahora comprueba ambas versiones con la altura física de cada papel: 297 y 279.4 mm. Ambas detectaron exclusivamente ID 0 y una pose compatible con el cuadrado de 100 mm; no es validación de una impresión física. C# compilado con Unity 6000.3.22f1 y salida 0; PDF revisado visualmente, sintaxis Bash comprobada. Fuentes/runtime AR, demos y configuración global de impresión sin cambios.

## M#[3] — resuelta: elección de impresora

Había dos impresoras configuradas y ninguna predeterminada. Se notificó la elección con el PDF preparado; el usuario eligió **Brother DCP-T510W**. No se envió nada a la DCP-T530DW.

Se ejecutó una sola vez:

```bash
lp -d Brother_DCP-T510W -n 1 -t 'Chupacabras - marcador carta 100 mm' \
  -o media=Letter -o PageSize=Letter -o print-scaling=none -o scaling=100 \
  -o fit-to-page=false -o sides=one-sided -o ColorModel=Gray \
  /home/cacawatin/code/unity/chupacabras/marker/03_marker_carta.pdf
```

Trabajo **Brother_DCP-T510W-43**. Consulta IPP: `completed`, `job-completed-successfully`, una impresión/hoja completada, Letter, scaling100 y sin ajuste. Opciones aplicadas exclusivamente a este trabajo; no se cambiaron preferencias globales ni se reenviaron copias.

## Evidencias y continuación

- `marker/03_marker_carta.pdf`, `.typ`, `_preview.png`, `SHA256SUMS_carta` y `README_carta.md`.
- `docs/evidencias/marker_checks_a4_20260923_222124.json` y `marker_checks_carta_20260923_222124.json`; log `marker_carta_20260923.log`.
- `docs/evidencias/marcador_carta.json` y `impresion_carta_job43.txt`.

**M#[2] parcial:** el usuario respondió explícitamente «Mide 100 mm (10 cm)» a la notificación de medida. La regla impresa queda confirmada por su comprobación física; falta el recorrido en G20. Esto no valida pose, escala virtual ni seguimiento AR. La app continúa desinstalada del móvil conforme a la petición previa. Paso 2 cerrado, paso 3 parcial; no se inicia 4.
