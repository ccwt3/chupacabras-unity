> **Referencia histórica anterior al cambio de alcance del 2 de octubre de 2026.**
> Resultados y entregas se conservan; sus próximos pasos, ventana/corto y
> solicitudes manuales no son instrucciones vigentes. Ver [estado](estado.md)
> y [migración a figura AR](figura_ar.md). M#[6] no se declara aprobada.

# Intercambio y bloqueo de 40 segundos — 24 de septiembre de 2026

Se ejecutaron únicamente los pasos 4 y 5. El cierre físico aceptado del paso 3
permanece vigente. No se inició el escenario definitivo del paso 6 ni se cambió
AprilTag. No se conectó el teléfono ni se instaló una APK en esta sesión.

## Entregas

- `Assets/Scenes/04_Exchange.unity`: muestra importada con rig, restricción
  horneada, blend shape, cámara y ventana.
- `Assets/Scenes/05_Blocking_r04.unity`: bloqueo vigente. Abrir con Unity
  **6000.3.22f1** y pulsar Play para ver la escena renderizada en la ventana.
  El marcador exterior es una referencia; esta escena no usa cámara física.
- `Assets/Exchange/04_intercambio.fbx` y `05_blocking_r04.fbx`: exportaciones
  explícitas con referencias `.json`. No hay importación automática de `.blend`.
- `Assets/Preview/`: materiales URP y RenderTextures. La ficción usa capa 8;
  la ventana y referencia usan capa 0. Tamaño de ventana 240 × 135 mm,
  2.4 veces el cuadrado de detección de 100 mm.
- `Assets/Scripts/SequencePreview.cs`: un reloj provisional de 40 s y un solo
  grafo Playables para todo el clip. La Timeline final corresponde al paso 17.
- Fuentes en `/home/cacawatin/code/blender/chupacabras/scenes/`:
  `04_intercambio.blend`, `05_blocking_r04.blend`.
- Video en `/home/cacawatin/code/blender/chupacabras/previews/05_blocking_r04.mp4`.
  Se genera con la cámara Unity, 960 × 540, 12 fps, 480 imágenes/40 s.

Se conservan las revisiones de bloqueo anteriores y ambas demos Blender.
R03 limita la exposición del acecho; R04 añade caída de costado y movimiento
provisional de patas. Las formas sirven para encuadre; no son recursos finales.
La oveja definitiva reutilizada con licencia sigue pendiente en el paso 7.

## Resultados

`docs/evidencias/2026-09-24_intercambio/`: metros/ejes, normales, 41 muestras
comparadas con Blender, duración 40 s y compresión de lana mediante blend shape.
Error máximo de vértice 3.46e-7 m. Grafo de ejecución: contacto 1.79e-7 m.
Las cuatro esquinas de una imagen de colores y su relación 16:9 pasan la prueba
de ventana. Se corrigió la ausencia de Animator en la raíz importada, creándolo
en la raíz común y desactivando root motion. Los intentos iniciales se conservan.

`docs/evidencias/2026-09-24_blocking_r04/checks.json`: contacto en 564 instantes,
error máximo 2.01e-6 m; cámara frente a Blender, <0.000463 m. El clip representa
40.0000038 s en float y el reloj recorta exactamente a 40 s. Máscaras de profundidad
320 × 180: 150 fotogramas con criatura/ojos ocultos (15–20), 114 con oveja oculta
(21.2–25) y 30 vacíos (39–40), todos sin píxeles visibles del objeto prohibido.
La oveja sí aparece antes/después; se detectan 7 píxeles de ojo en el acecho.
Tres vistas exteriores conservan idéntica, byte a byte, la imagen interna.

Las 32 aserciones existentes de `TrackingChecks` pasaron. Verificación estática
de la APK anterior 0.0.5 correcta y hash conservado. `verify_tracking_apk.py`
acepta ahora identificador opcional y omisión explícita de la comprobación de
permiso de cámara para el visor sin AR; sus valores por defecto no cambian.
Sintaxis Python/Bash revisada; sin suite de linters configurada.

No son pruebas físicas: no certifican brillo, fps, estabilidad ni compatibilidad
con el S23. La revisión de seguimiento 0.0.5 continúa sin nueva prueba móvil.
La APK de bloqueo tampoco valida seguimiento: reproduce la escena sin cámara AR.

## Repetir verificaciones y build

`bash scripts/verify_sequence.sh` ejecuta las verificaciones con el Editor exacto
y carpetas nuevas. Requiere GPU para máscaras/RenderTexture. Las muestras y escenas
ya creadas se conservan; no se ejecutan los generadores de escenas sobre entregas
existentes. `BlockingBuild.Configure` está reservado a una ruta/revisión nueva.

`bash scripts/build_blocking.sh` produce una APK nueva en `builds/android/` con
identificador `com.chupacabras.ar.blockingpreview`. Restaura nombre/identificador/
versión anteriores al terminar y no sustituye la escena de build de seguimiento.
Verificar con:

```bash
python scripts/verify_tracking_apk.py --package com.chupacabras.ar.blockingpreview \
  --no-camera-check builds/android/ARCHIVO.apk docs/evidencias/INFORME_NUEVO.json
```

Se limita el paralelismo de IL2CPP a dos trabajadores gestionados, como en las
builds anteriores. No se cambia de Editor ni configuración global. Los logs
brutos permanecen locales y sus copias `.gz` se versionan sin alterar su contenido;
los 480 PNG intermedios de cada video se conservan localmente y son reproducibles.

El estado general y los siguientes pasos **6 + 7** están en
[estado Blender](/home/cacawatin/code/blender/chupacabras/docs/estado.md).
No hay acciones manuales pendientes para cerrar esta sesión.

## Build final y conservación

APK R04: `builds/android/05_blocking_20260924_233453.apk`, 36 858 340 bytes,
SHA-256 `ba21a91015d6757d5d70a48e90020716cb192ce15dcafa275b5a48e2ff587cb3`.
Build Succeeded, 0 errores/0 advertencias, 1 min 02.46 s. Firma v2, zipalign
16 KB y ocho bibliotecas AArch64 correctas; no instalada. La APK R03 permanece.

Se archivaron los efectos de build en caché `.utmp` ya versionada y dos ajustes
automáticos de URP en `Library/Session_20260924_build_side_effects/`; después se
restituyeron solo esos cambios propios a su contenido inicial. La configuración
y escenas de seguimiento anteriores no forman parte del diff final.

Control de formato: diff de C#, Python, Bash, Markdown y JSON correcto.
El diff global registra 556 espacios finales en campos vacíos de YAML
serializado por Unity (.meta, escenas y recursos); se conserva el formato nativo
sin atribuirles un fallo de compilación ni alterar las entregas generadas.
