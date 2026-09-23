# Resultado de preparación de seguimiento — 23 de septiembre de 2026

Se trabajó **2 retomado + 3**; no se inició 4. Cambio: añadida cámara/pose y prueba de carga mínima para poder evaluar AprilTag en el G20 antes de producir recursos finales. La aplicación 02, los 62 archivos del paquete embebido y las demos Blender anteriores permanecen intactos. Estado general en `/home/cacawatin/code/blender/chupacabras/docs/estado.md`.

## Entrega

- `builds/android/03_tracking_20260923_213432.apk`: 36 259 124 bytes, SHA-256 `7b4f3fdd5c50c467fa4b4fc349399971affdb8f30ce21adc4fc3caaa1ac02000`; hash/licencias/informe acompañantes. `builds/` permanece ignorado por Git.
- `Assets/Scenes/03_TrackingProbe.unity`, fuentes C# y `.meta`, scripts de reproducción/inspección.
- `marker/03_marker_a4.pdf`, fuente CeTZ, PNG de revisión, original, comprobación de matriz y hashes.
- Guía física completa: [prueba_g20.md](prueba_g20.md).

## Verificación ejecutada

| Comprobación | Resultado y límite |
| --- | --- |
| Unity Android IL2CPP | 6000.3.22f1, build Succeeded, 0 errores/0 advertencias BuildReport, 5 min 05.97 s. |
| Geometría/estados/detector Linux | 29 aserciones; incluye la prueba estática positiva/negativa del paso 2. |
| PDF | A4, una página; matriz igual al original; detector encuentra solo ID 0, distancia sintética 0.2572058 m acorde con lado 100 mm. Impresión pendiente. |
| Firma/empaquetado | Firma v2 y zipalign de 16 KB correctos; ocho ELF AArch64 con segmentos LOAD ≥16 KB. Carga Android pendiente. |
| Manifiesto | Paquete separado de 02, mínimo 26/objetivo 35, permiso CAMERA, ARM64 provisional. INTERNET de desarrollo y permiso de receptor privado también presentes. |
| Editor visual | Adquisición, pérdida/pausa y recuperación/continuación; dos ejecuciones completas con salida 0. Capturas inspeccionadas. No son cámara física. |
| Integridad | APK 02 y 428 archivos Blender coinciden por SHA-256; Packages y escena 02 sin cambios Git. |
| Sintaxis | Bash de scripts 03 y 02, Python nuevo y cuatro scripts Blender: correctos. No hay linter ni otra suite propia configurados. |

Evidencias en `docs/evidencias/`: `tracking_checks_20260923_213418.json`, `marker_checks_20260923_214021.json`, `tracking_apk_verificacion.json`, logs de configure/build/preview y seis capturas `editor_tracking_*`. Capturas finales 214251/214252/214253 y CSV `editor_probe_20260923_214248_159/tracking.csv`.

Los avisos de login de servicios Unity y cierre `build-server` no impidieron la licencia, compilación ni captura; no se alteró la instalación ni se instaló .NET global. No hubo errores de la app en las capturas.

La primera captura usó el almacenamiento predeterminado del Editor; se corrigió su ruta bajo `UNITY_EDITOR` a `docs/evidencias/` y se repitió. El cambio no modifica el código Android del build entregado. El CSV de la primera ejecución también se conserva dentro del proyecto. La configuración del Editor y otras aplicaciones no se cambió.

La build regeneró `.utmp`, que el commit inicial había incluido como caché. Se conserva una copia de esos resultados en `Library/Step03GeneratedCmake/` y se restaura únicamente la caché versionada al estado de inicio; se apartan los tres archivos nuevos en esa misma copia. No se incluye ruido de compilación en el commit ni se borra el historial anterior. `Library/` está ignorado.

## Aceptación pendiente y reanudación

**M#[2] pendiente:** imprimir al 100 % y medir 100 mm, conectar/desbloquear Moto G20 y autorizar depuración USB/cámara, seguir el recorrido preparado. El agente consulta ABIs/API antes de instalar y recoge/analiza los registros. No se ejecutó ADB contra ningún teléfono en esta sesión.

Ni 2.4 ni 3.5 quedan completos. FOV 60° es provisional; lente real, orientación/reflejo, distorsión, pose/escala, recuperación y rendimiento siguen por validar. La RenderTexture contiene solo un cubo de prueba y no prueba el margen del corto definitivo. S23 pendiente de prueba física propia.

Próxima pareja: **2 retomado en 2.4 + 3 desde 3.2/3.3**, al responder M#[2]. No avanzar al 4 en esta sesión. No cambiar Editor, tecnología, duración 40 s ni decisiones de M#[1].


Actualización tras acceso al G20: paso 2 cerrado por ABI/API y carga nativa comprobados; cámara abierta, impresión/seguimiento aún pendientes. APK 0.0.3 corrige stripping y panel horizontal. App desinstalada al finalizar por solicitud del usuario. Véase [diagnóstico físico y limpieza](diagnostico_g20.md).
