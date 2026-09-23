# Paso 3: prueba física AprilTag en Moto G20

## Estado y alcance

Prueba de viabilidad **parcial en el G20**: ABI, instalación, cámara, impresión medida, detección física, giro y pérdida/recuperación comprobados con 0.0.4. Precisión/calibración y margen de rendimiento final pendientes. Véase [recorrido físico](seguimiento_fisico_g20.md). Paso 2 cerrado; paso 3 en curso, sin ejecutar 4. Véase [diagnóstico y limpieza](diagnostico_g20.md). Unity exclusivo 6000.3.22f1 en Linux, URP 17.3.0 y AprilTag 1.0.3 embebido original. El G20 conectado confirmó ARM64 y API 30; no extrapolarlo al S23. La escena de diagnóstico 02 y su APK se conservan.

La escena `Assets/Scenes/03_TrackingProbe.unity` muestra la cámara trasera, un cubo de **50 mm de lado** y dos ejes de 100 mm sobre ID 0. A su derecha se muestra una RenderTexture de 512 × 288 con un cubo giratorio generado en tiempo real. Es carga gráfica mínima para medir; no es el corto, no importa animaciones Blender y no completa el intercambio del paso 4 ni garantiza margen para los recursos finales.

## M#[2] — parcial: recorrido básico realizado; falta validación cuantitativa

Antes de realizarlo deben estar disponibles la APK 03 verificada, el PDF y esta guía. El agente instala, recoge registros y depura; el usuario no tiene que ejecutar comandos.

1. **Impresión:** imprimir `marker/03_marker_carta.pdf` en tamaño carta al **100 %**, sin «ajustar a página». Medir la regla y comunicar su longitud real: debe medir 100 mm. También comprobar 100 mm en el borde interior del marco negro continuo y 180 mm en el dibujo completo. Si la medida difiere, corregir impresión antes de comprobar escala. Montar plano, sin brillo ni pliegues, conservando márgenes blancos de al menos 15 mm. El marcador final del stand pertenece al paso 21.
2. **Acceso:** conectar el Moto G20 con un cable de datos y desbloquearlo. Si falta: Ajustes → Acerca del teléfono → pulsar siete veces «Número de compilación»; después Sistema → Opciones de desarrollador → Depuración USB. Aceptar la autorización de este equipo en pantalla. El agente consulta modelo, Android y ABIs, instala la APK adecuada y la abre. Si el sistema no admite ARM64, se detiene esta combinación y se documenta; no se cambia de tecnología automáticamente.
3. **Permiso:** conceder cámara cuando la app lo solicite. Si se deniega, comprobar el mensaje y usar «Reintentar»; ante denegación permanente, autorizar Cámara desde Ajustes de la app y volver. No dar por probado este caso con el Editor.
4. **Orientación:** apuntar a la hoja a unos 40–60 cm con buena luz. La imagen debe verse derecha, sin reflejo; usar texto asimétrico alrededor del marcador para comprobarlo. Girar teléfono a vertical y ambas posiciones horizontales, y volver. Si la cámara elegida no es la normal, el agente identifica la correcta con «Otra trasera» y sus registros.
5. **Pose y escala:** mantener el teléfono quieto 15 s en cada distancia aproximada de 35, 50 y 70 cm; medir desde la cámara a la hoja y comunicar las distancias. El cubo debe permanecer sobre el centro, sobresalir hacia el teléfono y conservar su lado de 5 cm; los ejes de 10 cm deben coincidir con el ancho/alto del cuadrado interior. Inclinar aproximadamente ±30° y desplazar lateralmente sin perder el tag. Registrar capturas con el botón cuando se indique.
6. **Pérdida y recuperación:** tapar toda la hoja durante 3 s y descubrirla, tres veces. Cubo y ventana deben ocultarse y el reloj detenerse; al recuperar, continuar desde el tiempo previo, sin duplicación ni reinicio. Repetir enviando la app a segundo plano y volviendo.
7. **Carga mínima:** con el tag visible y el teléfono apoyado, mantener 60 s con «RT: sí» y 60 s con «RT: no». El agente recoge tiempos y memoria. Esta comparación no sustituye la prueba final de cinco minutos del paso 22.

**Resultado que permite continuar:** evidencia real de ABI y carga nativa; imagen correcta; pose/escala repetibles; pausa y recuperación; comparación de rendimiento con RenderTexture. No se cierra AprilTag como viable por reconocer solo ID 0 ni por compilar.

Criterios de diagnóstico iniciales (no resultados ni acuerdos creativos): objetivo de 30 fps, detección ≥10 Hz, error de distancia ≤10 % con medidas disponibles; registrar dispersión de pose en tramos quietos y saltos visibles. Si hay errores sistemáticos de escala/proyección, preparar calibración física y repetir antes de aceptar 3.5. El FOV inicial de 60° **es una hipótesis**, no calibración G20. No pedir al usuario que adivine parámetros.

## Implementación y motivos

- `CameraGeometry`: deshace reflejo vertical antes de girar 0/90/180/270° en sentido horario; la misma imagen normalizada alimenta detector y fondo. Mantiene proporción mediante barras negras y el mismo FOV en la proyección 3D. Convierte FOV cuando intercambia ejes por giro de 90/270°.
- `TrackingProbe`: permiso Android, selección trasera con preferencia por `WebCamKind.WideAngle`, solicitud 320 × 240 a 15 fps (desde 0.0.4; antes 640 × 480) y registro de resolución realmente recibida. «Otra trasera» permite diagnosticar equipos cuya enumeración no identifique la lente normal. Sin frames durante 15 s, muestra error y permite reintentar.
- `TrackingState`: oculta y pausa en el primer frame procesado sin ID 0, o tras 0.4 s sin frames frescos. Segundo plano libera cámara/detector. La recuperación continúa el reloj. Se usan poses sin suavizado para observar su estabilidad real.
- Ventana de prueba sobre capa 8, cámara interna separada y RenderTexture. Se desactiva el render interno cuando no hay seguimiento o se desactiva RT. Materiales/shaders referenciados por escena para incluirlos en Android.
- Selector de FOV provisional 30–100° para diagnóstico. No se guarda como calibración universal ni se transfiere del G20 al S23. El wrapper fijado asume focales iguales y centro de imagen; no corrige distorsión. El teléfono determinará si basta o requiere calibración.
- App de desarrollo separada `com.chupacabras.ar.trackingprobe`, versión 0.0.5/5 (0.0.4/4 probada físicamente). No reemplaza `com.chupacabras.ar.probe` del diagnóstico 02. No se añade ARCore/AR Foundation ni otra versión del Editor.

## Evidencia recogida por la aplicación

En `Application.persistentDataPath/probe_<UTC>/`:

- `device.txt`: modelo, Android, Unity, cámara y enumeración trasera; resolución solicitada. Resolución real, orientación y FOV en CSV.
- `tracking.csv`: muestras a 10 Hz de estado, reloj, tiempo medio suavizado de frame, conversión/subida de imagen, tiempo de detección, RT, dimensiones y pose. Los tiempos corresponden a CPU; **no son medición directa de GPU**. Las columnas de pose conservan la última lectura cuando `visible=False`; excluir esas filas de estabilidad y distancia.
- Botón «Captura»: pantalla compuesta y frame normalizado de cámara. Solo guarda imágenes al solicitarlo; no graba video continuo. «Marcar siguiente prueba» permite separar tramos inicio/fijo/cerca/lejos/inclinar/ocultar/recuperar/rendimiento.

El agente usa `scripts/g20_probe.sh inspect`, `install <apk>` y `collect` después del acceso autorizado. El script exige exactamente un dispositivo autorizado, verifica que sea G20 y que anuncie `arm64-v8a` antes de instalar. No desinstala, borra datos ni limpia logcat. La instalación `-r` conserva datos de esta app de prueba. No ejecutar un script de instalación sobre otro dispositivo por conveniencia.

## Reproducción técnica por el agente

- `bash scripts/build_tracking.sh`: comprobaciones de Editor, configuración local y APK con nombre UTC nuevo. Paralelismo IL2CPP limitado como en paso 2; no cambia el sistema.
- `TrackingChecks.Run`: pruebas sintéticas de geometría, pérdida/recuperación y detector real Linux; informes fechados, sin sobrescribir evidencia del paso 2.
- `TrackingPreview.Capture`: abre la escena 03 temporalmente con fuente sintética solo en Editor, captura adquisición/pérdida/recuperación y termina. Ejecutar Unity exacto con `-batchmode -projectPath <raíz> -executeMethod TrackingPreview.Capture -logFile <log nuevo>`, sin `-nographics` ni `-quit`. No guarda el modo sintético en escena ni lo compila para Android.
- `MarkerChecks.Run`: detecta ID 0 sobre el PNG rasterizado desde el PDF y comprueba escala con carta (y A4 anterior) y 100 mm. Prueba de archivo, no de impresión.
- `TYPST_BIN=<typst> bash scripts/build_marker.sh`: genera en directorio nuevo el PDF CeTZ y su preview; Typst 0.14.2, CeTZ 0.5.2. Caché de paquetes dentro de `Library/TypstPackages`. El binario usado esta sesión se descargó a `/tmp`, sin instalar globalmente.

La escena 02 conserva su script `build_android.sh`; al invocarlo cambia la configuración activa a la prueba 02, y `build_tracking.sh` la devuelve a 03. Ninguna APK previa se sobrescribe.

## Referencias

AprilRobotics. (s. f.). *AprilTag 3: Pose estimation* [Código y documentación]. GitHub. https://github.com/AprilRobotics/apriltag

AprilRobotics. (s. f.). *tagStandard41h12/tag41_12_00000.png* [Marcador]. GitHub. https://github.com/AprilRobotics/apriltag-imgs/blob/master/tagStandard41h12/tag41_12_00000.png

Unity Technologies. (s. f.). *WebCamTexture.videoRotationAngle*. Unity 6.3 Documentation. https://docs.unity3d.com/6000.3/Documentation/ScriptReference/WebCamTexture-videoRotationAngle.html

Unity Technologies. (s. f.). *WebCamTexture.videoVerticallyMirrored*. Unity 6.3 Documentation. https://docs.unity3d.com/6000.3/Documentation/ScriptReference/WebCamTexture-videoVerticallyMirrored.html

Wolf, J., & fenjalien. (2026). *CeTZ* (0.5.2) [Paquete Typst]. Typst Universe. https://typst.app/universe/package/cetz/


Impresión conservada: carta, Brother DCP-T510W, trabajo 43; usuario confirmó regla de 100 mm (10 cm). Recorrido básico realizado y app retirada; consultar el cierre físico siguiente antes de pedir otra intervención. No repetir impresión ni pedir medidas exactas a mano sin preparar una referencia apoyada y reproducible.


Continuación física: el usuario confirmó cubo y ventana sobre la impresión. Se solicita captura 320 × 240 para reducir lectura, conversión y detección tras observar 18–24 fps a 640 × 480. Decimación 1 en imagen pequeña, 2 si el controlador entrega mayor resolución. Nueva columna `processed_frames` para medir frecuencia real del detector sin confundirla con muestreo CSV. FOV 60° sigue provisional: una referencia manual de 30 cm sin tramo estable sincronizado no basta para guardar calibración. APK 0.0.3 y evidencias se conservan.


Cierre del recorrido: el usuario confirmó giro correcto y tres ocultaciones/recuperaciones; CSV registra giros 0/90/0 y pausas del reloj sin reinicio. Aproximación de 50 cm aceptada como prueba funcional por petición del usuario; no se insiste en medidas exactas ni se inventa calibración. App 0.0.4 desinstalada, ausencia comprobada. Revisión 0.0.5 procesa cada imagen fresca de la cámara, sin segunda puerta temporal; su frecuencia real y coste requieren futura prueba física. Criterios cuantitativos todavía pendientes: precisión/estabilidad con referencia fiable, ambas horizontales, inclinaciones controladas, reanudación desde segundo plano, comparación RT 60 s sí/no y frecuencia de la última revisión. Hoy no se pide más intervención.
