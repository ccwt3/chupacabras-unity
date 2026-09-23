# Seguimiento físico G20 — 23 de septiembre de 2026

Sesión del paso 3 retomado; paso 4 previsto y todavía no iniciado. Preparación y evidencia anteriores intactas. Repositorio limpio al inicio en a94ac93. Solo se instala el paquete propio `com.chupacabras.ar.trackingprobe`, ausente al comenzar según `evidencias/g20_marcador_20260923/ownership.json`. Debe desinstalarse después de recoger datos, por petición del usuario.

## Marcador y primera detección

Carta impresa por Brother DCP-T510W, trabajo 43 de la sesión anterior. El usuario confirmó regla de 100 mm. En esta sesión autorizó USB y confirmó «Sí, aparecen ambos» ante cubo y ventana. Captura física inspeccionada en `evidencias/g20_collect_20260923_225015/screen.png`: cubo centrado sobre impresión y ventana a la derecha, cámara horizontal. Esto confirma detección real de ID 0, pero no certifica todavía orientación completa, escala, estabilidad ni recuperación.

APK inicial 0.0.3/3: `builds/android/03_tracking_20260923_220640.apk`; SHA-256 `68fd5ac2136d569286f959aa9db0c40985ab64d0f71b2bb1587348353dbbce42`. Instalación `g20_install_20260923_224921`. Sesión de aplicación `probe_20260923_224949_071`; recogida ampliada `g20_collect_20260923_225316`. Cámara 0, 640 × 480, giro 0, sin reflejo reportado. Tiempos de frame suavizados de aproximadamente 40–53 ms en tramos visibles; no alcanza el objetivo de 30 fps.

El usuario midió 30 cm desde cámara a hoja. La primera lectura cercana a esa confirmación rondaba 27.4 cm con FOV 60°, pero el CSV registra movimientos y no delimita un tramo quieto sincronizado inequívoco. No se fija calibración a partir de ella: la conclusión preliminar de que necesitaba ajustar FOV queda pendiente de una repetición controlada. Distorsión no calibrada; no extrapolar al S23.

## Revisión 0.0.4/4

Solicita 320 × 240 @15 para reducir lectura y procesamiento. Detector sin decimación en captura pequeña; mantiene decimación 2 si Android entrega una resolución mayor. Registra dimensiones reales. Añade `processed_frames` al CSV para calcular frecuencia efectiva del detector. Conserva FOV ajustable de 60° provisional. No cambia paquete, tecnología ni bibliotecas. Corrige rótulo obsoleto de ABI en nuevos informes de build, conservando informes históricos.

La suite verifica además reconocimiento, profundidad métrica y ausencia de falsos positivos sobre blanco a 320 × 240. 32 aserciones aprobadas en Unity 6000.3.22f1. Son comprobaciones sintéticas; la aceptación sigue dependiendo de la revisión física.

## Punto intermedio de ejecución (superado por el cierre posterior)

Build/verificación APK, reinstalación temporal, distancia controlada, orientación/inclinación, pérdida/recuperación y comparación con/sin RT. Recoger evidencias y desinstalar antes de cerrar. M#[2] en curso; M#[1] y M#[3] permanecen resueltas.


## Build y verificación de revisión

APK `builds/android/03_tracking_20260923_225712.apk`, versión 0.0.4/4, 36 273 984 bytes, SHA-256 `9cca2d400c7ff44e28bb65dde4ce59137f18d6de87ec02bd06f3944274ff81f8`. Build 1 min 11.94 s, cero errores/advertencias. El campo Bytes de BuildReport incluye recursos intermedios; el tamaño APK real lo verifica el script. Firma, zipalign de 16 KB y ocho bibliotecas AArch64 verificados. Informes `tracking_checks_20260923_225703.json`, `tracking_configure_20260923_225651.log`, `tracking_build_20260923_225651.log`, `g20_apk_20260923_225712.json`.

`scripts/summarize_tracking.py <csv> --start <segundos> --end <segundos>` reproduce estadísticas del tramo explícito; excluye poses no visibles y advierte que el CSV puede repetir poses. La medida de frame es suavizada y no corresponde a perfilado GPU. Baseline de 640 × 480 en `g20_marcador_20260923/baseline_640.json`.

La compilación regeneró `.utmp`; se conservó en `Library/GeneratedCmake_20260923_225651/` y se restauró únicamente ese ruido generado, partiendo de repositorio limpio. Sin cambios ajenos.


## Tramo controlado de 30 cm con revisión 0.0.4

Usuario: «Quieto a 30 cm». Ventana de lectura CSV [84.7163, 114.8737] s, sesión `probe_20260923_230009_349`, recogida `g20_collect_20260923_230204`; 246/246 muestras visibles y RT activa. Mediana frame suavizado 33.322 ms (~30 fps), imagen 4.881 ms, detector 14.743 ms; frecuencia efectiva de procesamiento 8.99 Hz (objetivo inicial ≥10, todavía no satisfecho). Profundidad mediana 0.34555 m, desviación 0.00302 m, error respecto a referencia manual +15.18 %. No se acepta escala; hace falta segunda referencia. La foto muestra hoja inclinada respecto a cámara, de modo que el supuesto de paralelismo solicitado no se logró plenamente. No tratar dispersión de posición como ruido exclusivo del detector: también incluye movimiento físico.

Evidencia y resumen: `g20_marcador_20260923/30cm_revision.json`. M#[2] notificada para medir 50 cm desde la lente al centro y mantener posición. Se conserva FOV 60° hasta contrastar referencias.


## Referencia aproximada de 50 cm y límite manual

El usuario indicó que no puede medir exactamente y quedarse completamente quieto; solicitó aceptar una comprobación aproximada y continuar/cerrar. Se respeta: no se piden más medidas precisas. Confirmó aproximadamente 50 cm; recogida `g20_collect_20260923_230431`, tramo [250.0956, 264.6908] s, 122/122 muestras visibles, RT activa. Mediana z 0.42283 m, frame 33.322 ms, detector 14.5705 ms, frecuencia 7.81 Hz. La captura muestra detección, cubo y ventana. **La referencia aproximada no permite calcular error físico ni ajustar una calibración.** Las dos medidas manuales no justifican un FOV específico; se conserva 60° provisional. Criterio de precisión/estabilidad estricta pendiente; no atribuir movimiento manual al detector.

Se localizó una segunda puerta temporal de 1/15 s además de `didUpdateThisFrame`: con frames de 33.32 ms podía saltar imágenes frescas. Revisión 0.0.5 elimina esa puerta para cámara real (la solicitud sigue a 15 fps), conservándola solo para preview sintético. La frecuencia resultante debe verificarse en Android; no dar por corregidos 10 Hz solo por inspección de código.

M#[2] solicita únicamente un giro vertical/horizontal y tres ocultaciones de 3 s, ofreciendo terminar sin hacerlas. Después de recoger se limpia el teléfono. No se exige mantenerlo exactamente quieto ni ejecutar comandos.


## Giro, pérdida y recuperación; cierre del teléfono

El usuario respondió **«Sí, gira bien y desaparecen/regresan»** al recorrido solicitado: giro vertical/horizontal y tres ocultaciones de 3 segundos. No se le pidieron más acciones físicas. Recogida final `g20_collect_20260923_230650`: CSV confirma rotaciones 0 → 90 → 0 y múltiples pérdidas/recuperaciones. Resumen `g20_marcador_20260923/transitions.json`; por ejemplo, entre 339.4665–342.1989 s el reloj queda en 189.0553 s y al recuperar continúa (189.1220 s), sin reset. No se atribuye cada transición a una de las tres ocultaciones sin sincronización; también hay pérdidas breves durante manipulación. No se registró la otra orientación horizontal ni se prueba ausencia universal de saltos.

**Limpieza completada a las 23:07:15 UTC.** `g20_marcador_20260923/cleanup.json`: `adb uninstall` devuelve `Success`; paquete, proceso y carpeta externa ausentes (códigos 1 sin stderr). La app no existía al iniciar. No se desinstalaron otras apps ni se modificaron configuraciones globales. Se notificó que puede desconectar el teléfono; no se pedirá nueva autorización USB para compilar la revisión final local.

**Paso 3 parcial.** El recorrido funcional funciona, pero no hay calibración métrica fiable ni comparación RT sí/no de 60 s. La revisión de frecuencia 0.0.5 queda preparada sin instalar; no atribuirle la validación de 0.0.4. Pendientes también ambas horizontales, inclinación controlada y segundo plano. Se cierra esta tanda a petición de limitar la intervención manual. Paso 4 no iniciado; próximas dos etapas: 3 retomado + 4. AprilTag permanece candidato y S23 sin prueba. No cambian duración de 40 s, demo de 35 s ni fuentes Blender.


## Entrega final local y verificaciones

APK **0.0.5/5**: `builds/android/03_tracking_20260923_230736.apk`, **36274616 bytes**, SHA-256 `752eb7cd342644761dfa965701991de57a8cf752d7b54c236db24976af3aa755`. Unity 6000.3.22f1/Linux, IL2CPP/ARM64: build correcto, 0 errores y 0 advertencias, 1 min 05.00 s. 32 aserciones de Editor aprobadas otra vez (`tracking_checks_20260923_230726.json`). Firma, zipalign16KB y ocho bibliotecas AArch64 correctos (`g20_apk_20260923_230736.json`). **No instalada ni probada en Android**: la evidencia física corresponde a 0.0.3/0.0.4.

Sintaxis Bash, compilación sintáctica Python y `git diff --check` sobre fuentes/documentación aprobadas; no hay linter propio configurado. Logs de última compilación `tracking_configure_20260923_230715.log`, `tracking_build_20260923_230715.log`. Se archivaron cachés regeneradas propias bajo `Library/GeneratedCmake_20260923_230715/`, incluida caché Python; entregas previas preservadas. El verificador APK ahora describe su límite estático sin afirmar obsoletamente que el ABI del G20 sigue desconocido.

Todas las respuestas manuales recibidas durante la sesión quedan registradas; validaciones físicas faltantes se aplazan, sin interpretar silencio como aprobación. No queda limpieza pendiente. Esta sesión trabaja solo el paso 3; 4–24 no iniciados. Commit local de cambios propios, sin push; el estado Blender registra su identificador.


Revisión del commit `41dc0ba`: `git diff HEAD^ HEAD --check -- . ':!docs/evidencias/**'` correcto. El chequeo global señala 305 avisos de espacios finales/líneas vacías en logs originales de Unity/ADB y memoria; se conservan sin reformatear para preservar la evidencia. No son errores de compilación ni de código. Árbol limpio tras commit, sin push.
