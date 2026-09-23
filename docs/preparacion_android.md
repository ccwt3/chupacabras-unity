# Preparación Unity/Android — paso 2

Este proyecto pertenece a `/home/cacawatin/code/unity/chupacabras`; las fuentes Blender permanecen en `/home/cacawatin/code/blender/chupacabras`. El estado general y el límite de pasos se consultan en `docs/estado.md` de Blender.

## Alcance

Proyecto mínimo URP con una prueba de AprilTag sobre imagen incluida. La prueba compartida por Editor y APK exige ID 0, pose finita delante de cámara y cero detecciones al procesar una imagen blanca. No usa cámara física, no implementa seguimiento AR ni reproduce el corto. Es la preparación técnica del paso 2; el paso 3 deberá integrar cámara, proyección y prueba real.

No se ha solicitado acceso al G20. No se afirma compatibilidad del G20 ni del S23 por compilación o por ejecución en Editor.

## Configuración

- Unity **6000.3.22f1**, revisión `1c726e1fb402`, usando exclusivamente el lanzador instalado acordado. Su opción Vulkan preexistente afecta al Editor; la APK usa OpenGLES3 provisionalmente.
- URP **17.3.0** incluido con el Editor. Renderizador Universal, sin HDR, MSAA 1 y sin VSync; objetivo de ejecución 30 fps, todavía sin medición móvil.
- `jp.keijiro.apriltag` **1.0.3** embebido, revisión `fd6dd4698c9c6d2dc4a5e676beeab7f620006c78`. Paquete original con licencia BSD-2-Clause y `.meta`, sin cambios; procedencia en `apriltag_provenance.json`.
- Burst **1.8.30** fijado directamente; Mathematics **1.3.2**, Collections **2.6.8** en el bloqueo.
- Versiones efectivamente resueltas en `Packages/packages-lock.json`; conservarlo junto con manifest, `.meta` y `ProjectSettings`.
- IL2CPP con stripping administrado Low y ARM64 **provisional**: es el único binario Android del candidato. La ABI real del sistema G20 aún debe consultarse antes de cerrar aceptación. No inferirla del procesador.
- SDK mínimo 26 y objetivo 35, JDK 17.0.18, NDK r27c (27.2.12479018), build-tools 36.0.0 de la instalación existente. No se instaló otro Editor.
- Aplicación de desarrollo `com.chupacabras.ar.probe`, versión 0.0.1/1, firma de depuración. Sin credenciales de distribución en el proyecto.
- Orientación automática provisional para la pantalla de diagnóstico; orientación final del stand pendiente. Sin permiso de cámara hasta la integración del paso 3.

## Reproducción

```bash
bash scripts/build_android.sh
```

El script usa la ruta exacta del Editor, guarda logs en `docs/evidencias/`, deja la caché Gradle dentro de `Library/` y crea una APK con fecha UTC en `builds/android/`. El método de compilación rechaza sobrescribir APK existentes. Requiere el Editor instalado y la licencia ya activada; la primera resolución de paquetes/Gradle puede necesitar red.

`ProjectBuild.ConfigureAndVerify` prepara ajustes y comprueba la biblioteca Linux con el detector real. Crea la escena `Assets/Scenes/02_DetectorSmoke.unity` solo si falta; no reemplaza una escena existente. `ProjectBuild.BuildAndroid` construye esa escena. Ambos rechazan versiones diferentes del Editor.

La APK ejecuta la misma prueba al arrancar y guarda `detector-smoke.json` en el almacenamiento privado de la app; etiqueta `CHUPACABRAS_DETECTOR_OK` en logs o excepción si falla. Esto queda listo para diagnóstico posterior, pero no se ha ejecutado en un teléfono.

## Diagnóstico de memoria y alcance local

El primer intento fue terminado con código 137 durante `IL2CPP_CodeGen`; el kernel registró explícitamente OOM. El segundo intento utiliza `DOTNET_PROCESSOR_COUNT=2` y `-job-worker-count 2` dentro del script, junto con stripping Low. Durante el diagnóstico se restringió temporalmente la afinidad del árbol de compilación. Una vez superada la generación de código y comprobada la memoria estable del compilador nativo, se restauraron las CPU disponibles. El script final limita trabajadores administrados sin imponer afinidad ni requerir taskset. El resultado final se recoge en el informe de la build.

Estos límites afectan solo al árbol de procesos lanzado por el script; no cambian swap, memoria ni configuración global del equipo. Los logs y el informe del intento fallido se conservan. No instalar un SDK .NET global por el aviso del cierre del Editor sobre `build-server`: la compilación usa las herramientas incluidas en Unity.

Las licencias se conservan en `docs/licenses/AprilTag-wrapper.txt` y `docs/licenses/AprilTag-native.txt`; acompañar cualquier copia de la APK con estos avisos.

## Pose: detalle que debe conservarse

En esta revisión, `Runtime/Unity/Internal/PoseEstimationJob.cs` calcula `height / 2 / tan(fov / 2)`: el argumento es **FOV vertical en radianes**. El README del wrapper dice horizontal/grados y no corresponde a ese código. Se siguió la implementación y su ejemplo. Los 60° y 0.10 m de la prueba son parámetros sintéticos; no son calibración ni tamaño físico validado. La orientación, rotación de sensor, reflejo y proyección de cámara deben resolverse con la imagen real en el paso 3.

## Imagen y binarios

`Assets/Resources/AprilTagFixture.png` deriva del ID 0 de `tagStandard41h12`; escala por vecino más cercano y fondo blanco. Procedencia en `fixture.json`. No usar esta captura como marcador físico a escala: el marcador imprimible y su medida se prepararán en el paso 3.

Los binarios originales incluyen Linux x86-64 y Android AArch64. Los segmentos LOAD de Android están alineados a 0x4000 (16 KB); esto es inspección ELF, no prueba de carga en un sistema Android de 16 KB. La carga Linux se comprueba por detección real en Unity.

## Pendiente de aceptación física

Preparar cámara trasera y permisos, marcador imprimible con medida definida, instrucciones concretas y nueva APK de seguimiento. Entonces solicitar acceso físico como M#[2], identificar modelo/Android/ABIs por ADB y comprobar instalación, carga nativa, cámara, pose, escala, pérdida/recuperación y margen de RenderTexture. El corto y los recursos finales siguen pendientes según el plan.

## Referencias

Takahashi, K. (s. f.). *jp.keijiro.apriltag* (revisión fd6dd4698c9c6d2dc4a5e676beeab7f620006c78) [Código fuente]. GitHub. https://github.com/keijiro/jp.keijiro.apriltag/tree/fd6dd4698c9c6d2dc4a5e676beeab7f620006c78

AprilRobotics. (s. f.). *apriltag-imgs: tag41_12_00000.png* [Imagen de marcador]. GitHub. https://github.com/AprilRobotics/apriltag-imgs/blob/master/tagStandard41h12/tag41_12_00000.png

Unity Technologies. (s. f.). *Unity Editor command line arguments reference*. Unity 6.3 Documentation. https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html

Unity Technologies. (s. f.). *Android requirements and compatibility*. Unity 6.3 Documentation. https://docs.unity3d.com/6000.3/Documentation/Manual/android-requirements-and-compatibility.html

## Resultado comprobado — 23 de septiembre de 2026

- Proyecto abierto con 6000.3.22f1; compilación Android final **Succeeded, 0 errores, 0 advertencias de BuildReport**. Duración del intento exitoso: 13 min 05.7 s. El primer intento fallido por OOM queda conservado.
- APK: `builds/android/02_detector_20260923_205436.apk`, **35 986 932 bytes**, SHA-256 `0a150d6bd72390e8d20d9acb5d33ccb90d85ff826e23cfe7998be0e4646f51f9`.
- `apksigner verify`: firma v2 válida. `zipalign -c -P 16 4`: correcto. Manifiesto: mínimo API 26, objetivo 35, solo ARM64. Permiso INTERNET de la build; todavía sin CAMERA.
- Ocho bibliotecas nativas empaquetadas, todas con segmentos LOAD alineados a 16 KB. Incluye `libAprilTag.so`; su sección de código `.text` coincide con el original. Unity elimina símbolos al empaquetar, por lo que el archivo completo tiene un hash distinto.
- Dependencias sin ARCore ni AR Foundation; paquete AprilTag embebido original (62 archivos comparados). Revisión y licencias conservadas.
- `docs/evidencias/detector-editor.json` y `detector-playmode.json`: ID 0, una detección, pose finita y cero detecciones sobre blanco. Plataforma **LinuxEditor**, no Android físico.
- Captura final `docs/evidencias/editor_diagnostic_20260923_211159.png`, inspeccionada visualmente: mensaje de éxito, patrón y límite de prueba estática legibles.
- La captura inmediata produjo una excepción interna de `UnityEditor.Search.SearchDatabase` al arrancar Play Mode antes del fin del inicio. `DiagnosticPreview.Capture` ahora difiere la entrada usando `EditorApplication.delayCall`. La reapertura con ese cambio terminó con salida 0 y sin esa excepción; se conservan los logs anteriores.
- C# compilado por Unity y sintaxis de `scripts/build_android.sh` comprobada con `bash -n`. No había suite propia ni linter configurados: las pruebas técnicas están en `DetectorSmokeTest` y se ejecutan durante la preparación y al iniciar la escena.
- SDK/JDK/NDK comprobados por la build real. Avisos de servicios de login y cierre `build-server` en logs no impidieron usar la licencia existente, compilar ni capturar; no se pidió activación ni se instaló .NET global.

Reproducir la captura de Editor, sin `-nographics` ni `-quit` (el automatismo termina el proceso):

```bash
/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity -job-worker-count 2 -batchmode -projectPath /home/cacawatin/code/unity/chupacabras -executeMethod DiagnosticPreview.Capture -logFile docs/evidencias/preview_nueva.log
```

Ejecutar desde la raíz Unity; elegir un nombre de log nuevo para conservar evidencias anteriores. Las capturas llevan fecha UTC. El informe de detección de Editor se actualiza en cada ejecución; los logs fechados conservan los resultados previos.

**Estado del paso 2:** preparación, carga Linux y compilación realizadas; aceptación completa pendiente de consultar las ABIs reales del G20. Coordinar esa comprobación al preparar la prueba del paso 3, después de disponer de cámara, marcador e instrucciones. No se ha ejecutado el paso 3 en esta sesión.
