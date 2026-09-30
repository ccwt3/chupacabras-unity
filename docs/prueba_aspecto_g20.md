# Prueba física del aspecto — 30 de septiembre de 2026

Se retomó **12.2 / M#[4]**. El usuario conectó el Moto G20 y autorizó instalar
la prueba y retirar todo lo instalado al terminar. No se inició el paso 13.

## Primera APK: 0.0.12

Comprobado G20, Android 11/API 30, ARM64 y páginas de 4096 bytes. Antes de
instalar no había paquetes `com.chupacabras`. Instalación y arranque correctos.
El detector nativo pasó su prueba incluida y abrió Camera 0 a 320 × 240.
Después adquirió el AprilTag físico; se registraron 39,03 s de reloj visible
en la muestra recogida, con 244 lecturas de seguimiento válido.

El usuario observó la figura con las patas hacia él y el panel mirando hacia
arriba. La captura confirma el error: la composición inicial estaba diseñada
en el plano XY del tag. Con papel sobre la mesa, la figura no quedaba de pie
y el panel estaba acostado. **Esta revisión física no pasó M#[4].**

Rendimiento observado durante ese tramo: mediana de `frame_ms` 41,644 ms;
p95 129,041 ms, incluyendo transiciones/adquisición. Son muestras del contador
suavizado de la app, no tiempos GPU ni prueba de cinco minutos. No se declara
30 fps sostenidos; la optimización completa conserva el paso 22.

## Corrección local: escena R04, APK 0.0.13

La revisión intermedia de esta preparación fue
`Assets/Scenes/12_AppearanceAR_r04.unity` con
`Assets/Appearance12/AppearanceStudy_r04.prefab`. Figura de pie sobre la normal
del marcador y centrada hacia su borde superior; panel vertical a la derecha,
con el frente dirigido al observador situado delante del papel sobre la mesa.
El giro se aplica por encima de la conversión de ejes FBX, conservándola.
No se animan la cámara AR ni la figura exterior.

La figura se separa 290 mm del centro hacia el borde posterior y queda 5 mm
sobre el plano. El panel está a 280 mm a la derecha, 100 mm hacia atrás y su
centro a 75 mm sobre el plano. Sus dimensiones siguen siendo 240 × 135 mm.
Esta disposición evitaba cubrir el dibujo; la revisión física posterior motivó R05.
No resuelve automáticamente el montaje vertical del cartel previsto para 21.

R02 conservó posición lateral e inclinó el panel; R03 atendió la ubicación
superior y puso el panel vertical. R04 amplió la separación. Estas iteraciones
no sustituyen la escena inicial ni las demos; R02/R03 quedan como diagnóstico.

La prueba proyectada ahora usa el eje vertical de la mesa y compara cada
triángulo real con el cuadrilátero protegido de 220 mm. El antiguo rectángulo
envolvente marcaba falsas intersecciones al mirar oblicuamente. R04 pasa las
vistas frontal/izquierda/derecha, independencia de cámaras y adquisición,
pérdida, pausa y recuperación sintéticas. C# compila y clang-format/Bash pasan.

APK: `builds/android/12_appearance_20260930_224951.apk`, 38.676.904 bytes.
SHA-256: `8fc790f044acd00055690bd0ebc96e0b110379d1e52972547076d48d8201c576`.
Unity 6000.3.22f1, versión 0.0.13/código 13 (**continúa siendo el paso 12**).
Firma, permiso CAMERA, ARM64 y ocho ELF/ZIP alineados a 16 KB comprobados.

Evidencia física inicial: `docs/evidencias/g20_aspecto_20260930_224133/`.
Preparación corregida: `docs/evidencias/appearance_20260930_224859/`.
APK verificada: `docs/evidencias/apk_12_appearance_20260930_224951/`.

## Acceso USB y limpieza

Durante las ejecuciones del Editor se cerró/reabrió el servidor ADB. El teléfono
pasó a no autorizado; el usuario indicó que no aparecía la solicitud. Se intentó
reconectar la sesión desde el equipo; después ADB dejó de mostrar dispositivos.
Se pidió reconectar físicamente el cable dentro de **M#[4]**. No se cambiaron
claves ADB ni ajustes globales. El intento de detener la app se rechazó por
falta de autorización; en ese momento no se consideró desinstalada ni detenida.

Se recuperó ADB reiniciando el servidor con `ADB_VENDOR_KEYS` apuntando a la
clave existente del equipo; no se alteró esa clave. APK 0.0.13 instalada y
abierta correctamente. La captura `sample_225402/screen.png` demuestra figura
de pie y panel levantado. En ese momento faltaba la valoración sobre posición/brillo;
la aclaración posterior y el cierre de M#[4] constan en el apartado final.

Se registró un `GL_INVALID_OPERATION` nativo al comenzar el seguimiento; apareció
una sola vez en los logs recogidos y la composición siguió renderizándose.
No se detectaron excepciones C# en ese registro. La causa del aviso OpenGL sigue
sin resolver: revisar/reproducir durante diagnóstico móvil, no declararlo corregido.
No ocultar el aviso ni interpretar las capturas como ausencia total de fallos.

**Limpieza completada a las 22:56:08 UTC:** `adb uninstall` devolvió `Success`;
no quedan paquetes `com.chupacabras`, proceso de la app ni carpeta externa
`/sdcard/Android/data/com.chupacabras.ar.appearance12`. Android elimina los datos
internos al desinstalar; no se inspeccionaron directamente sin root. Las APK se
instalaron por streaming, sin copiar archivos a Descargas. Evidencia: `cleanup.json`.
Fuentes, APK y registros locales se conservan para continuar el proyecto.

## Aclaración final y cierre: R05 / 0.0.14

El usuario aclaró que «encima» significa **centrado directamente sobre el símbolo**,
no en el borde superior del papel. R05 coloca la figura de pie en el origen del
marcador. Su superposición virtual sobre el dibujo es intencional y sustituye el
criterio anterior de exclusión para la figura. El panel sigue fuera del dibujo;
el detector recibe los píxeles de cámara sin composición virtual.

Entrega vigente: `Assets/Scenes/12_AppearanceAR_r05.unity` y
`Assets/Appearance12/AppearanceStudy_r05.prefab`.
APK 0.0.14: `builds/android/12_appearance_20260930_225854.apk`, 38.676.888 bytes.
SHA-256: `4c4b33846b193684dd925fe9a162f3d2da8c52fe0aaba32994b0ce028b275551`.
Build con 0 errores/0 advertencias, firma/ARM64/alineación verificados.
Pruebas y capturas nuevas: `docs/evidencias/appearance_20260930_225754/`.

Instalada, abierta y capturada en el G20. El usuario confirmó **«Sí, ahora está
como quería»** a ubicación y lectura de figura/panel. **M#[4] resuelta y paso 12
cerrado con este alcance visual.** No se inició 13 ni se amplía la aceptación a
actuación definitiva, calibración o rendimiento sostenido.

Registro final: 879 muestras, 628 con seguimiento, **76,3558 s de reloj visible**;
mediana del contador suavizado de cuadro visible 39,407 ms. No prueba 30 fps
sostenidos. Sin excepciones C# ni aviso OpenGL en este registro de 0.0.14; la
causa del aviso único de 0.0.13 no se ha identificado, observarla en las pruebas
móviles posteriores. Captura vigente: `sample_230235/screen.png` dentro de la
carpeta física; `sample_230204` aporta otra vista.

**Limpieza final de 0.0.14: 23:02:40 UTC.** `uninstall` correcto, ningún paquete
`com.chupacabras`, proceso ni carpeta externa restantes. Evidencia separada:
`cleanup_0014.json`. No quedan APK copiadas a Descargas; instalación por streaming.
La limpieza anterior (`cleanup.json`) corresponde a 0.0.13 y se conserva.

Se explicó que las vistas de pastoreo/agarre son únicamente muestras de iluminación.
El corto definitivo de 40 s sigue pendiente de los pasos 13–19. Próximos: **13 y 14**.
