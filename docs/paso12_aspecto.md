# Paso 12 — aspecto nocturno y composición AR

Preparación del 30 de septiembre de 2026. Unity **6000.3.22f1** desde Linux.
El paso permanece abierto hasta **M#[4]** en el Moto G20. No se inició 13.

## Entrega y alcance

`Assets/Scenes/12_AppearanceAR.unity` y
`Assets/Appearance12/AppearanceStudy.prefab`: figura original del acabado 09,
quieta a la izquierda; panel de 240 × 135 mm a la derecha, renderizado en tiempo
real a 960 × 540. Ambos siguen la misma pose AprilTag. La cámara interna y el
escenario se mantienen fuera de la raíz del marcador, en la capa 8.

La ventana alterna cada seis segundos entre tres **estudios de iluminación**:
pastoreo, agarre y vista de espinas/lana. Se evalúan poses de los clips de rigs
10 y 11, con contacto R03 y triangulación fija. La cámara cambia deliberadamente
entre muestras; esto no es la cámara ni la actuación del corto definitivo.
El corto sigue siendo de 40 s; la demo de 35 s permanece independiente.

Materiales URP mate, ojos amarillos sin iluminación, luz principal azulada con
sombras duras a 1024, relleno sin sombras y ambiente frío. Sin trama adicional,
postprocesado ni texturas nuevas: se prioriza la lectura del modelo. El pipeline
se copia a un recurso propio y se selecciona únicamente durante esta muestra.
El fondo de cámara y el panel son unlit, sin recibir las luces del corto.
La figura conserva la conversión de ejes del FBX y mide aproximadamente 12 cm
de alto; comprobar su escala física con M#[4], no asumir calibración.

`TrackingProbe` admite una referencia opcional a la muestra; la escena anterior
sin esa referencia conserva cubo, panel y diagnóstico originales. Se reutilizan
captura, detector, orientación, FOV provisional y estado de seguimiento. Al perder
pose, oculta ambos elementos y detiene el reloj de la muestra. No adelanta la
integración definitiva del paso 20 ni cierra M#[5].

## Reproducción

Desde esta raíz Unity, ejecutar `bash scripts/build_appearance.sh`. Usa solo el
Editor fijado, verifica tres vistas internas y tres exteriores, prueba el ciclo
adquirir/perder/recuperar en Play Mode y compila una APK con nombre único bajo
`builds/android/`. Cada ejecución crea su carpeta de evidencia. La configuración
inicial rechaza sobrescribir una escena/recurso ya generado. No ejecutar
`Configure` sobre entregas existentes.

No se modifican versiones de paquetes, detector ni recursos anteriores. Los
ajustes de PlayerSettings y pipeline usados por la compilación se restauran al
terminar. El paquete independiente `com.chupacabras.ar.appearance12` evita
reemplazar la prueba 03. Versión de la muestra: 0.0.12, código 12, ARM64/IL2CPP.

## M#[4] — prueba física preparada

Utilizar **el marcador carta ya impreso y medido**, familia tagStandard41h12,
ID 0, 100 mm entre esquinas de detección y dibujo completo de 180 mm.
El PDF conservado es `marker/03_marker_carta.pdf`; no hace falta reimprimir si
se conserva la impresión validada. Mantener 20 mm libres alrededor del dibujo.

Cuando se solicite M#[4]:

1. Conectar y desbloquear el Moto G20; aceptar USB o cámara si aparece la solicitud.
   El agente verifica modelo/ABI, instala y abre esta APK; no necesitas comandos.
2. Apuntar a la impresión, preferiblemente con el teléfono horizontal, hasta ver
   la figura a la izquierda y el panel a la derecha. Retroceder lo suficiente para
   incluir ambos y todo el marcador. Mantener el FOV provisional durante la revisión.
3. Observar las tres muestras durante al menos 18 s. Indicar si se distinguen ojos
   amarillos, espinas, lana, dientes y sombras, y si el brillo resulta suficiente.
4. Mover el teléfono suavemente a izquierda/derecha (aproximadamente ±15°) y algo
   hacia arriba. Confirmar perspectiva de la figura, escala junto al panel y que
   ningún elemento virtual tape el dibujo, sus esquinas ni márgenes. La cámara
   interna debe conservar su vista durante cada muestra.
5. Comunicar los fallos concretos observados. El agente recoge capturas y logs,
   corrige si hace falta y retira únicamente la aplicación de esta prueba al cerrar.

Resultado requerido: lectura útil en pantalla y composición simultánea sin tapar
el tag dentro de ese recorrido. Una captura del Editor no da esa aceptación.
Calibración exacta/FOV, cinco minutos sostenidos, costo del corto final, páginas
Android de 16 KB y S23 conservan sus validaciones posteriores. No pedir M#[5] aún.

## Resultado comprobado y entregas vigentes

- APK: `builds/android/12_appearance_20260930_223107.apk` (38.676.908 bytes),
  SHA-256 `5f96b6b3632ab7f07f72d64d5b89e00664509263e54a49d0b0553aa473f7ae07`.
- BuildReport: Unity 6000.3.22f1, **0 errores / 0 advertencias**. El tamaño
  total de BuildReport incluye datos de construcción; el tamaño de APK es el anterior.
- Firma verificada, ARM64, permiso CAMERA, ocho bibliotecas ELF y empaquetado
  alineados a 16 KB. Esto no demuestra ejecución física en dispositivos de 16 KB.
- Capturas vigentes de aspecto/composición:
  `docs/evidencias/appearance_20260930_222740/` (`checks.json`, `cinema_0..2.png`,
  `composition_front/left/right.png`). Prueba conservadora de límites proyectados:
  ningún mesh exterior invade el cuadrado protegido de 220 mm en esas tres vistas.
  La cámara del corto produce los mismos píxeles al cambiar pose exterior.
- Ejecución sintética vigente: `docs/evidencias/2026-09-30_visual/runtime03/`.
  Adquisición, pérdida, reloj detenido y recuperación comprobados en Play Mode.
  La captura ajusta el viewport al destino de evidencia 960 × 540; no depende
  de las dimensiones de Game View. No contiene la interfaz OnGUI ni cámara física.
- `revision01`, `revision02`, `runtime02` y el `runtime.json` fallido de la
  carpeta `appearance_20260930_222740` documentan iteraciones; no son cierre físico.
  La primera automatización agotó el tiempo esperando captura diferida; runtime02
  pasó comportamiento pero tenía proporción incorrecta en la imagen de evidencia.
- 32 aserciones existentes de tracking correctas; C# compilado, Bash verificado
  y clang-format de los scripts nuevos correcto. El verificador APK se reproduce
  con `python scripts/verify_appearance_apk.py` (elige la APK 12 más reciente y
  crea una carpeta nueva, sin reemplazar evidencia).

**12.1 preparada. 12.2 / M#[4] pendiente; 12.3 pendiente de sus resultados.**
No se instaló esta APK ni se probó el G20/S23 en esta sesión. No se inició 13.

La regresión de la escena original 03 también pasó adquisición/pérdida/recuperación
en Play Mode: `docs/evidencias/2026-09-30_visual/tracking_regresion.log` y
`docs/evidencias/editor_tracking_20260930_223507_0.png` (más estados 1 y 2).
Los archivos de caché `.utmp` rastreados que regeneró la compilación se restituyeron
a sus bytes previos; no forman parte de la entrega ni del commit.
