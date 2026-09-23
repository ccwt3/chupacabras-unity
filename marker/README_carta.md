# Marcador vigente: tamaño carta

Usar **03_marker_carta.pdf**, una hoja carta (215.9 × 279.4 mm), al 100 %, sin ajuste automático. La aplicación conserva el lado de detección de 100 mm; dibujo total 180 mm y familia/ID originales. No es necesario recompilar ni reinstalar la app por este cambio de papel.

Fuente CeTZ: `03_marker_carta.typ`; preview: `03_marker_carta_preview.png`. Generador predeterminado: `scripts/build_marker.sh`. Se redujeron únicamente dos separaciones de texto para caber en la hoja más corta. Márgenes laterales 17.95 mm alrededor del dibujo.

Los archivos A4 anteriores y su README/hashes se conservan como entrega histórica. Los nuevos hashes están en `SHA256SUMS_carta`; la matriz coincide exactamente con la versión A4.

El 23 de septiembre de 2026 se envió una copia a la Brother DCP-T510W elegida por el usuario, trabajo 43. CUPS informó una hoja completada, Letter, scaling100, print-scaling none y fit-to-page false. **El usuario confirmó que la regla impresa mide 100 mm (10 cm).** Falta validar escala virtual y seguimiento en el G20. Resultado y evidencia en `docs/marcador_carta.md`.
