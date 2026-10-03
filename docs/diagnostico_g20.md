> **Referencia histórica anterior al cambio de alcance del 2 de octubre de 2026.**
> Resultados y entregas se conservan; sus próximos pasos, ventana/corto y
> solicitudes manuales no son instrucciones vigentes. Ver [estado](estado.md)
> y [migración a figura AR](figura_ar.md). M#[6] no se declara aprobada.

# Primer acceso físico al Moto G20 — 23 de septiembre de 2026

Se retomaron exclusivamente **2.4 y 3.2–3.3**. El usuario autorizó probar la app del proyecto y pidió desinstalar lo añadido al terminar. También pidió notificaciones para las acciones manuales; se usaron para autorización USB y disponibilidad del marcador. Respondió que aceptó USB y que **todavía no tiene el marcador impreso**.

## Alcance sobre el teléfono

Solo se instala el paquete de desarrollo `com.chupacabras.ar.trackingprobe`; al inicio no existía, ni tampoco `com.chupacabras.ar.probe`. No se instala ARCore, no se usa root, no se actualiza Android, no se reinicia el equipo ni se cambian ajustes globales. Se recogen únicamente datos de la app, propiedades técnicas necesarias, métricas de su proceso y capturas de su prueba. La app se detiene mientras se compila para liberar la cámara. La desinstalación y ausencia posterior se registran al cierre.

## Datos comprobados

- Modelo: `motorola moto g(20)`.
- Android 11, API 30, compilación informada por Unity `RTAS31.68-66-3/66-3`.
- ABIs del sistema: `arm64-v8a,armeabi-v7a,armeabi`; lista de 64 bits: `arm64-v8a`.
- Página de memoria: 4096 bytes. No es una prueba de Android con páginas de 16 KB.
- Cámara seleccionada: `Camera 0`, trasera WideAngle. También enumera Camera 2/4 WideAngle y Camera 3 UltraWideAngle. Captura real 640 × 480, solicita y configura 15 fps.
- Permiso de cámara concedido; la imagen aparece en pantalla. La primera muestra se tomó en horizontal: giro 0, sin reflejo vertical reportado. Esto no valida las tres orientaciones ni la calibración.

## Fallo observado y corrección local

APK 0.0.2 instala y abre, pero registra dos errores `Can't add component because class 'MeshCollider' doesn't exist!`. El build había eliminado el tipo usado indirectamente al crear los quads con `GameObject.CreatePrimitive`; el Editor no reproducía ese stripping. El detector sí procesó imágenes, sin tag presente.

Se añadió `Assets/link.xml` para conservar MeshCollider/BoxCollider y MeshFilter/MeshRenderer requeridos por las primitivas. Se mantuvo stripping Low y el resto de configuración. La corrección pertenece solo a esta app.

El panel tapaba casi toda la vista en horizontal porque escalaba exclusivamente por ancho. `TrackingProbe.OnGUI` ahora limita la escala con ancho y alto. La APK de corrección es versión 0.0.3/código 3; se conserva la 0.0.2 anterior.

Se añadió una comprobación nativa inicial con `AprilTagFixture`: ID 0 y pose finita, luego cero detecciones sobre blanco. Guarda `detector-device.json` y una etiqueta de éxito/error. Aunque se ejecute en el G20, esta comprobación usa **una imagen incluida**, no demuestra seguimiento de una impresión física.

## Evidencias del primer arranque

- `g20_inspect_20260923_220306/`: propiedades y ausencia inicial de ambos paquetes del proyecto.
- `g20_install_20260923_220328/`: instalación `Success` y lanzamiento.
- `g20_arranque_20260923_2206/`: log exclusivo del PID de la app, memoria, CSV y datos de cámara. Los archivos IL2CPP de caché obtenidos al recoger datos se conservan en Library, fuera del commit de fuentes.
- 425 muestras con cámara durante aproximadamente 52.57 s; ninguna detección de marcador y reloj 0. Mediana del tiempo de frame suavizado 33.322 ms, conversión 9.267 ms, detector 9.251 ms. Incluye arranque/picos; no es medición de RT ni de GPU ni aceptación de rendimiento final.

## Pendiente

**M#[2] parcial:** acceso USB y permiso/cámara realizados. Falta imprimir el PDF al 100 %, medir 100 mm y hacer el recorrido de distancias, inclinaciones, orientación, pérdida/recuperación y carga RT. El usuario confirmó que todavía no dispone de la impresión. No se declara completado el paso 3 ni adoptado AprilTag como viable para producción.

La limpieza pedida implica que habrá que reinstalar la APK del proyecto cuando se retome la prueba física. No instalar nada antes de que el marcador esté disponible.

## Referencia

Unity Technologies. (s. f.). *GameObject.CreatePrimitive*. Unity 6.3 Documentation. https://docs.unity3d.com/6000.3/Documentation/ScriptReference/GameObject.CreatePrimitive.html


## Resultado final comprobado y limpieza

APK corregida `builds/android/03_tracking_20260923_220640.apk`, 36 273 532 bytes; SHA-256 `68fd5ac2136d569286f959aa9db0c40985ab64d0f71b2bb1587348353dbbce42`. Unity 6000.3.22f1, build Succeeded, 0 errores/0 advertencias, 1 min 31.70 s. Firma v2, zipalign de 16 KB y ocho bibliotecas AArch64 verificados. El texto «ABI PROVISIONAL, G20 pending» del informe `.build.txt` proviene de la plantilla anterior; los registros físicos de esta sesión confirman ARM64 y carga nativa. No modificar retroactivamente ese informe.

La APK 0.0.3 se instaló/abrió en el G20. `detector-device.json` declara **Android**, una detección de ID 0 con pose finita delante de cámara y cero sobre blanco. El log no contiene errores de Unity ni excepciones de la app durante la captura; desaparece el fallo de MeshCollider. La captura física muestra el panel reducido y cámara abierta. Se observan metadatos de rotación 90° y 0°; no se comprobó correspondencia óptica completa con patrón asimétrico ni estabilidad/escala.

Con ello se cierra **2.4** para este G20: arquitectura/API, build, instalación y carga nativa reales. **3.3 queda parcial y 3.4–3.5 pendientes.** No confundir la prueba de imagen incluida con seguimiento físico ni certificar el candidato para producción.

Se ajustó `g20_probe.sh`: lanzamiento directo mediante `am start -W`, registros solo del PID de esta app, captura de pantalla solo si está en primer plano y recogida limitada a sus carpetas `probe_*`. La primera ejecución con Monkey enumeró tombstones existentes; eso no constituye prueba de caída de esta app. Se evitó esa enumeración en la segunda instalación.

**Limpieza realizada:** `adb uninstall com.chupacabras.ar.trackingprobe` devolvió `Success`. Después, `pm path` y `pidof` confirman paquete y proceso ausentes, y tampoco existe `/sdcard/Android/data/com.chupacabras.ar.trackingprobe`. No se usó `-k`, por lo que se eliminaron los datos de nuestra app. No se desinstaló ninguna app preexistente. Evidencia: `docs/evidencias/g20_limpieza_20260923.json`.

Resultados: `g20_collect_20260923_221046/` (log, captura, datos de la prueba y resumen), `g20_install_20260923_221014/`, `g20_apk_20260923_220640.json`, `tracking_checks_20260923_220625.json` (29 aserciones) y logs de compilación/configuración `220611`. Se verificó sintaxis Bash y XML; no hay linter adicional configurado. Las entregas 02/03 anteriores se conservan.

**Siguiente pareja:** 3 retomado (con impresión medida y nueva instalación temporal) + 4 después, respetando las dependencias y el límite de la siguiente sesión. No se inició 4 ahora. Notificar al usuario cada acción manual necesaria y desinstalar lo añadido al finalizar las pruebas, según su instrucción vigente.
