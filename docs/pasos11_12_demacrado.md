> **Referencia histórica anterior al cambio de alcance del 2 de octubre de 2026.**
> Resultados y entregas se conservan; sus próximos pasos, ventana/corto y
> solicitudes manuales no son instrucciones vigentes. Ver [estado](estado.md)
> y [migración a figura AR](figura_ar.md). M#[6] no se declara aprobada.

# Modelo demacrado: revisión de rig y aspecto

2 de octubre de 2026. Solo pasos **11 retomado y 12 retomado**. Unity
**6000.3.22f1**, desarrollo/compilación desde Linux. Sin ARCore, paquetes nuevos
ni cambios en instalaciones/configuración global.

## Entregas

- `Assets/Rigs/11_rigs_contacto_demacrado_r01.*`: rig, FBX, referencia de
  deformación y contacto; escena homónima en `Assets/Scenes/`.
- `Assets/Rigs/11_rig_chupacabras_poses_demacrado_r01.*`: controles extremos;
  escena homónima. Ambas muestras duran seis segundos y no son el corto final.
- **`Assets/Scenes/12_AppearanceAR_demacrado_r02.unity`** y
  **`Assets/Appearance12DemacradoR02/AppearanceStudy.prefab`**: revisión vigente.
- Fuente Blender: `/home/cacawatin/code/blender/chupacabras/scenes/09_chupacabras_demacrado_r02.blend`.
  Rigs editables, FBX/JSON y MP4 en esa raíz; detalle en `docs/rigs_demacrado.md`.

Se reemplazan la figura exterior, la criatura de la muestra de pastoreo y el
rig de contacto, conservando transformaciones de R05. Figura de pie centrada
sobre el símbolo; panel lateral vertical de 240 × 135 mm, RT 960 × 540.
Los estudios del panel duran seis segundos cada uno, con cambios de cámara
intencionales entre estudios; no son la cámara continua ni actuación de 40 s.
La demo Blender de 35 s permanece independiente.

Se copian materiales y pipeline a una carpeta nueva: piel seca gris verdosa,
cuencas oscuras, ojos amarillos unlit. Se conserva la iluminación de revisión.
No se cambia el detector, su captura, el FOV provisional ni el runtime de
seguimiento. El modelo nuevo contiene 30.880 triángulos, no los 10.550 previos;
el costo móvil de esa diferencia sigue sin medirse.

`AppearanceBuild.CreateLeanRevision` sustituye instancias conservando su capa,
posición, rotación y escala. **R01 fue un diagnóstico fallido**: el prefab de
criatura heredó capa 8 y falló `Exterior layer` antes de compilar. Se corrigió
la conservación de capa y se generó R02; R01 y su log quedan identificados y
conservados. R05/0.0.14 permanece histórica y no contiene la malla demacrada.

## Reproducción y verificaciones

Los scripts conservan sus destinos históricos por defecto. Para construir la
nueva revisión desde esta raíz:

```bash
export CHUPA_APPEARANCE_SCENE=Assets/Scenes/12_AppearanceAR_demacrado_r02.unity
export CHUPA_APPEARANCE_FOLDER=Assets/Appearance12DemacradoR02
export CHUPA_APPEARANCE_VERSION=0.0.15
bash scripts/build_appearance.sh
python scripts/verify_appearance_apk.py
```

El script crea evidencias con fecha y APK con nombre único. Para regenerar
recursos, fijar primero **otra** escena/carpeta inexistentes y ejecutar
`AppearanceBuild.CreateLeanRevision` con el Editor exacto; no ejecutar ese
método sobre una entrega ya existente. Para repetir solo verificaciones,
`AppearanceBuild.Verify` y `AppearancePreview.Capture` aceptan las mismas
variables y `CHUPA_APPEARANCE_EVIDENCE` debe apuntar a una carpeta nueva.

Evidencias: `docs/evidencias/2026-10-02_rigs_demacrado/` y
`docs/evidencias/2026-10-02_aspecto_demacrado_r02/`. En rigs se comparan 181
cuadros por muestra, duración, compresión de lana, apoyo, dientes contra
triángulos importados y raíz AR independiente. En aspecto se comprueban
cantidad de triángulos de figura/rig, cuencas, capas, figura estática centrada,
panel fuera del marcador, tres planos y tres perspectivas. Los píxeles
internos permanecen idénticos al mover la raíz AR. Play Mode verifica
adquisición/pérdida, reloj detenido y recuperación sobre imagen sintética.
Nada de esto constituye validación física.

## M#[6] — revisión física de la malla nueva, pendiente

M#[4] permanece resuelta para R05/0.0.14 y el modelo anterior. **M#[6]** es la
prueba nueva del paso 12.2; no reabre decisiones de colocación ya confirmadas.
M#[5] sigue reservada para la integración del paso 20, no se solicita ahora.

Marcador preparado y conservado: `marker/03_marker_carta.pdf`,
tagStandard41h12 ID 0, 100 mm entre esquinas de detección, dibujo 180 mm.
Usar la impresión ya medida y plana, con sus márgenes; no hace falta reimprimir.

Cuando se solicite M#[6]:

1. Conectar/desbloquear el **Moto G20** y aceptar USB/cámara si lo solicita.
   El agente identifica modelo/ABI, instala la APK indicada y abre la muestra.
2. Apuntar al marcador, con el teléfono preferentemente horizontal y encuadrando
   la figura y el panel. Observar al menos **18 s**, los tres estudios completos.
3. Comprobar que se distinguen **ojos amarillos, costillas, espinas, dientes y
   lana**, y que la figura/panel tienen brillo útil. Informar cualquier elemento
   que desaparezca en sombras o cualquier parpadeo.
4. Mover el teléfono suavemente a ambos lados y un poco arriba. La figura sigue
   sobre el símbolo y cambia de perspectiva; el panel permanece al lado.
   Su cámara interna no debe seguir el movimiento del teléfono.
5. El agente registra capturas/logs, corrige si hace falta y retira únicamente
   la app de prueba `com.chupacabras.ar.appearance12`, verificando la limpieza.

Resultado requerido: legibilidad suficiente de la nueva anatomía y materiales
en **G20 real**, conservando la composición acordada. No pide elegir de nuevo
la ubicación ni la tecnología. Si hay fallos, no cerrar 12.3 ni iniciar 13.
No se han instalado apps en esta sesión. Calibración, rendimiento sostenido,
prueba física Android 16 KB y S23 siguen pendientes de sus etapas.

## APK lista para M#[6]

`builds/android/12_appearance_20261003_012410.apk`, versión **0.0.15**,
41.400.738 bytes. SHA-256:
`b10e73187f762d6691ecea2319fcb8f1676df37283531bee41d4bafaae5ae4a5`.
BuildReport: **0 errores, 0 advertencias**, Unity 6000.3.22f1. Firma,
permiso CAMERA, ARM64 y ocho ELF/empaquetado a 16 KB verificados en
`docs/evidencias/2026-10-02_aspecto_demacrado_apk/`.
La APK previa 0.0.14 conserva su SHA-256
`4c4b33846b193684dd925fe9a162f3d2da8c52fe0aaba32994b0ce028b275551`.
No se ha probado físicamente 0.0.15; paso 12.2/12.3 pendiente **M#[6]**.

Regresiones existentes: `bash scripts/verify_rigs.sh` pasa oveja 10, controles
11 anteriores y contacto R03; evidencia `docs/evidencias/rigs_20261003_012606/`.
Las 32 aserciones de tracking pasan en verificaciones de rigs y aspecto.
C# compilado, clang-format, sintaxis Bash y Python correctos. No se configuró
una suite/linter adicional. El verificador histórico vuelve a serializar la
escena 10; se guardó el diff diagnóstico y se restituyeron sus bytes iniciales.
También se restituyeron exclusivamente cachés `.utmp` rastreadas regeneradas
por el build. Los tres cambios ajenos preparados en ProjectSettings se conservan.
