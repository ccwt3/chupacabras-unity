# Pasos 8–9: importación del chupacabras original

Sesión del 24 de septiembre de 2026 (México). Punto inicial: commit
`0dde327db0619141ea64c4761cd1f280a0dcd803`, árbol limpio. Unity **6000.3.22f1**.

## Entregas y motivo

Se importan anatomía y acabado originales producidos en la raíz Blender
`/home/cacawatin/code/blender/chupacabras`. Allí están los generadores, fuentes,
FBX/JSON, vistas y documentación completa: `docs/chupacabras_modelo.md`.

- Forma vigente: `Assets/Scenes/08_chupacabras_forma_r03_contexto_r02.unity`.
- Acabado vigente: **`Assets/Scenes/09_chupacabras_acabado_contexto_r02.unity`**.
- Prefabs y seis materiales portables en `Assets/Creature/<hito>/`.
- `Assets/Editor/CreatureBuild.cs`: importación y verificación reproducibles.
- `scripts/verify_creature.sh`: reabre ambas escenas con el Editor exacto y
  guarda evidencia en carpetas nuevas, sin sobrescribir las anteriores.

La forma original y R02 se conservan, con fallos documentados; no son el cierre
vigente. La cola del modelo largo sobresalía del granero; la oveja se veía
entre las patas. Los contextos nuevos giran la criatura durante 15–20 s y elevan
la cámara interna a 2,4 m durante 20–25 s. La postura de acecho se mantiene hasta
14 s. El contexto final `_contexto_r02` usa el giro corto a −180° y desplaza
la criatura a y=9,3 m desde 14 s; las garras del acabado necesitan 0,25 m
adicionales hacia el fondo antes de ese momento. Así se evita atravesar la
pared: cero solapamientos AABB con cajas conservadoras de paredes/tejado en
1.200 cuadros, verificados en Blender. Contextos anteriores conservados. Se conservan intactos el bloqueo R04 y el contexto 06.

El verificador compara límites calculados de vértices transformados, evitando
la sobreestimación de `Renderer.bounds` para ojos rotados. Los materiales
se reconstruyen explícitamente en URP; no hay texturas ni pelo simulado.

## Evidencia y resultados

Informes y capturas en:

- `docs/evidencias/2026-09-24_08_chupacabras_forma_r03_contexto_r02/`.
- `docs/evidencias/2026-09-24_09_chupacabras_acabado_contexto_r02/`.

Forma: 8.486 triángulos/12 mallas. Acabado: 10.550 triángulos/17 mallas/6 materiales.
Límites de vértices importados a menos de 0,00000043 m del original. UV completos,
materiales compatibles, 40 s. Máscaras por cuadro: 150 de criatura oculta,
114 de oveja oculta y 30 finales vacíos. Cuatro píxeles de ojos en acecho para
el acabado; oveja visible antes/después. Contacto entre referencias:
error máximo 0,00000240 m en 564 cuadros de reproducción. Las 32 aserciones
existentes de seguimiento pasan. Las capturas fueron inspeccionadas.

Regresión: `bash scripts/verify_assets.sh`, con resultados independientes
registrados en `docs/evidencias/2026-09-24_chupacabras_cierre/regresion.json`: ambos pasan,
la oveja conserva cuatro poses con error máximo 0,11 mm. El archivo existente no tiene bit ejecutable,
por lo que se invoca con Bash. C# compilado; Bash y Python con sintaxis correcta.
Formato de código nuevo: `clang-format --style='{BasedOnStyle: Microsoft, ColumnLimit: 100}'`.

## Límites y continuación

Es una muestra de volumen: la oveja es la malla real evaluada, con giro rígido
heredado. Durante el arrastre queda elevada; las referencias de boca/cuello
coinciden, pero **no prueban mordida deformable ni apoyo en el suelo**. Eso
requiere bajar/articular la cabeza y adaptar lana/cuerpo en pasos 10–11.
No se crea el rig final ni se sustituye esta muestra por la actuación definitiva.

No se modifica la escena de seguimiento AprilTag, no se añade ARCore y no hay
APK ni prueba física nuevas. La ventana de revisión es 3D en tiempo real en
Editor. Los conteos de geometría y máscaras no son rendimiento del G20 ni
validación del S23. La luz/brillo definitivos permanecen en el paso 12.

## Revisión de presentación y cámara

Tras revisar la captura de referencia, el usuario pidió que la composición
final incluya una figura quieta del chupacabras en 3D, anclada a la pose del
AprilTag, junto al panel lateral que reproduce la secuencia. Los pasos 8–9
entregaron los modelos y escenas de revisión; **esa figura exterior aún no
está añadida a la aplicación ni comprobada físicamente**. El plan ahora la
sitúa en la previsualización visual del paso 12 y en la integración de tracking
del paso 20. La cámara del corto se suavizará en las animaciones nuevas de
13–14: anticipación antes del segundo 20 y entrada lateral o desde detrás del
granero. R04 conserva el corte como bloqueo histórico.

Pruebas manuales futuras, después de preparar build e instrucciones:
M#[4] revisará tamaño, perspectiva y legibilidad de la composición en el G20;
M#[5] probará pose conjunta, pérdida/recuperación y reinicio durante el paso 20.

Próximos dos pasos: 10, rig/deformaciones de oveja; 11, rig y contacto del
chupacabras. No se iniciaron en esta sesión. No quedan acciones manuales
indispensables. Demos y entregas anteriores se conservan; no se hizo push.
