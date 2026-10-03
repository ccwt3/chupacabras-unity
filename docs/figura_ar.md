# Preparación de la figura AR

2 de octubre de 2026. **Documento de planificación; ninguna modificación de
runtime realizada en esta sesión.**
[Plan canónico](../../../blender/chupacabras/docs/plan_secuencia.md) ·
[Estado](estado.md) · [Ficha visual](../../../blender/chupacabras/docs/ficha_produccion.md).

## Qué se reutiliza y qué falta

La base existe: cámara y normalización de imagen, AprilTag, pose/proyección,
raíz anclada, gestión de pérdidas y build Android desde Linux. Conservar Editor
6000.3.22f1, URP, paquetes y marcador medido existentes. El presupuesto real de
la nueva figura debe medirse en Moto G20; no extrapolarlo del Editor o del S23.

Punto de partida: `Assets/Scenes/12_AppearanceAR_demacrado_r02.unity` y
`Assets/Appearance12DemacradoR02/AppearanceStudy.prefab`, incluida su figura
estática. No son todavía una escena independiente del panel.

- `Assets/Scripts/TrackingProbe.cs`: `CreateView` crea cámara/fondo y anclaje,
  llama a `appearance.Attach` y retorna. Si `appearance` es nulo, crea el cubo
  y la ventana de diagnóstico. **Asignar null no elimina el cine.** `Update`
  llama a `Present`; la UI conserva controles de estudios, reloj y RT.
- `Assets/Scripts/AppearanceStudy.cs`: `Attach` crea la RenderTexture 960×540
  y el PlayableGraph; ocultar visualmente el panel no elimina esa carga.
- `Assets/Scripts/CameraGeometry.cs` y pruebas en
  `Assets/Editor/TrackingChecks.cs`: preservar geometría de cámara,
  adquisición/pérdida, timeout y segundo plano. Los checks actuales también
  cubren comportamiento narrativo histórico que debe seguir funcionando allí.
- Leer builders y scripts de tracking/aspecto antes de implementar F1. Crear
  ruta estática mínima compartiendo el seguimiento; evitar una copia del motor.

Crear escena/prefab/build nuevos, con nombres propuestos en el plan; no
sobrescribir las revisiones antiguas. Mantener identidad/versionado explícitos
y destinos únicos. Revisar BuildReport y dependencias: la build nueva no debe
arrastrar oveja, escenario, cámara cinematográfica ni Timeline/Playables por
referencias indirectas. Conservar recursos necesarios para la propia cámara AR.

## Verificación prevista

Para F1: compilación exacta, pruebas sintéticas de estados, figura única,
centrado y capturas desde distintos ángulos; APK base y dependencias verificadas.
No atribuir calidad artística ni validación física a esos checks. F2–F4
producirán el modelo refinado y su integración; los criterios completos viven
en el plan canónico para evitar divergencias.

La validación física nueva es M#[7], preparada antes de pedir intervención:
APK candidata con hash, marcador actual y guía de observación. Medir cámara,
detector y figura juntos, con referencia de 30 fps sostenidos, sin confundir Hz
de detección con fps de render. Evaluar rostro/materiales, ángulos, escala,
pérdida/recuperación y segundo plano. M#[8] conserva el ensayo separado del S23.
No pedir conexión ahora ni marcar una prueba como aprobada por documentación.

## Entregas anteriores

Se conservan escenas, scripts, prefabs, exportaciones, APKs y evidencias.
`builds/android/12_appearance_20261003_012410.apk` (0.0.15) aún incluye panel;
es una referencia histórica sin prueba física registrada para esa versión.
El marcador sigue siendo `marker/03_marker_carta.pdf`, tagStandard41h12 ID 0,
100 mm de detección y dibujo de 180 mm. No es un QR convencional.

Las antiguas M#[5]/M#[6] se sustituyen por cambio de alcance, sin aprobación
ni ejecución ficticia. Los documentos anteriores conservan sus resultados y
comandos de reproducción histórica, pero no sus instrucciones de continuación
como tareas vigentes. No borrar recursos antiguos para reducir la build nueva.
