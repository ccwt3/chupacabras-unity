# Pasos 6–7: escenario y oveja reutilizada

Sesión local del 24 de septiembre de 2026 (logs finales del 25 en UTC).
Punto inicial: `fde1295`, árbol limpio, Unity **6000.3.22f1**.
Solo se ejecutaron los pasos 6 y 7. Fuentes Blender y estado general permanecen
bajo `/home/cacawatin/code/blender/chupacabras`.

## Entregas vigentes

- `Assets/Scenes/06_Environment.unity`: nuevo escenario estático con el contexto
  animado de 40 s. Usa proxies de los animales, no la oveja de producción.
- `Assets/Environment/`: 2915 triángulos, diez mallas y diez materiales de color;
  geometría importada en metros, sin texturas ni animación del escenario.
- `Assets/Scenes/07_Sheep_r04.unity`: oveja quieta reutilizada, situada en el nuevo
  escenario. `Assets/Sheep/Sheep_Quaternius_r04.prefab` conserva el esqueleto.
- `Assets/Sheep/07_oveja_r04_poses.fbx`: prueba de aptitud de 6 s, independiente
  del corto de 40 s. Fuente: Quaternius, Farm Animals Pack (2018), CC0 1.0.
  `License_Quaternius.txt` es el texto distribuido por el autor.

El recurso fuente, ZIP original, licencia, copia de páginas y manifiesto SHA-256
están en `assets/oveja/` de Blender; adaptación y procedencia detalladas en
`docs/asset_oveja.md` de esa raíz. La oveja tiene 612 triángulos, 24 huesos, dos
materiales y altura 1.18 m. Se limitaron 35 vértices a cuatro influencias para
reproducir la misma deformación en ambos motores. No se reconstruyó el modelo.
Compresión de lana y rig final quedan en el paso 10.

Se corrigió una intersección del granero con el giro del bloqueo mediante un
nuevo contexto: granero y acecho previos a 20 s se desplazan 3.2 m hacia el fondo.
Cámaras, salto, contacto, arrastre y salida conservan las curvas anteriores.
No se guardaron cambios en las escenas de pasos 4–5 ni en sus FBX.

## Verificaciones

- `2026-09-24_escenario/checks.json`: escenario 2915 triángulos/10 renderers;
  150 fotogramas de criatura oculta, 114 de oveja oculta y 30 de campo vacío.
  Ojo visible durante acecho (4 píxeles a 320 × 180); oveja antes/después.
- `2026-09-24_oveja_r04/checks.json`: cuatro poses evaluadas con Playables,
  error máximo frente a Blender **0.00011044 m**, altura **1.17999947 m**.
  Apoyo mínimo −0.00008874 m, dentro de tolerancia de 1 mm.
- `tracking_checks_20260925_000035.json`: 32 aserciones existentes correctas.
- Capturas finales inspeccionadas; los logs y capturas de intentos anteriores
  también se conservan. `07_Sheep.unity` y `2026-09-24_oveja_r03/` están superados.
- Reapertura independiente Blender, hashes de demos/hitos y sintaxis Python/Bash
  correctos; C# compilado por el Editor y formateado con clang-format.
- No hay un linter de proyecto configurado. Se valida el diff de fuentes/docs;
  el YAML generado por Unity conserva su formato, incluidos espacios finales.

`EnvironmentBuild` y `SheepBuild` crean escenas nuevas; rechazan sobrescribirlas.
`Verify` reabre sin guardar. Ejecutar `bash scripts/verify_assets.sh` repite las
pruebas con el Editor exacto y carpetas de evidencia nuevas. Las capturas requieren
GPU: no usar `-nographics`. No se usan fuentes Blender por importación automática.

El verificador de skin utiliza `BakeMesh(..., true)` para compensar escalas y
`forceMatrixRecalculationPerRender` al capturar múltiples poses por actualización;
las capturas iniciales tenían matrices visuales antiguas. No se altera el
controlador de reproducción ni el seguimiento AR.

Unity normalizó de nuevo dos ajustes de URP al abrirse. Se conserva copia en
`Library/Session_20260924_assets_side_effects/` y se restituyen únicamente los dos
cambios propios a su contenido inicial, evitando mezclar configuración ajena al
alcance. Ningún archivo previo tenía cambios al comenzar. Los logs `.gz` son
copias exactas de los brutos; cachés y entregas APK siguen fuera de Git.

## Límites y continuidad

No hay APK nueva ni prueba física en esta sesión. Las escenas de revisión no
abren la cámara ni integran la secuencia con AprilTag. El paso 20 sigue pendiente;
no se añade ARCore. La iluminación final está en 12 y el presupuesto de rendimiento
móvil requiere medición real. G20/S23 no se consideran validados por estos renders.
No se solicita teléfono ni otra acción manual.

Retomar en **8.1**, modelado original del chupacabras junto a la oveja; siguiente
pareja **8 + 9**. No se inició ninguno de esos pasos aquí.

## Referencias

Quaternius. (2018, junio). *Farm Animal Pack* [Modelos 3D]. https://quaternius.com/packs/farmanimal.html

Quaternius. (2018, 8 de junio). *LowPoly Animated Farm Animal Pack* [Modelos 3D]. OpenGameArt. https://opengameart.org/content/lowpoly-animated-farm-animal-pack

Creative Commons. (s. f.). *CC0 1.0 Universal*. https://creativecommons.org/publicdomain/zero/1.0/
