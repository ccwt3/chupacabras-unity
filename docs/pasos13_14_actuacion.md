> **Referencia histórica anterior al cambio de alcance del 2 de octubre de 2026.**
> Resultados y entregas se conservan; sus próximos pasos, ventana/corto y
> solicitudes manuales no son instrucciones vigentes. Ver [estado](estado.md)
> y [migración a figura AR](figura_ar.md). M#[6] no se declara aprobada.

# Pasos 13–14: calma y ataque importados

Unity **6000.3.22f1**, desde Linux. Fuentes Blender en
`/home/cacawatin/code/blender/chupacabras`; aplicación AR en esta raíz.
El usuario aplazó el cierre físico de 12/M#[6] para avanzar 13–14. No se
modifica el seguimiento AprilTag ni se añade ARCore.

Abrir `Assets/Scenes/13_calma_r07.unity` o
`Assets/Scenes/14_ataque_r07.unity` y pulsar Play. La primera muestra 0–20 s;
la segunda 0–25 s, con cámara continua y ataque desde 20 s. Son previews
independientes: la duración definitiva sigue siendo 40 s y 25–40 s aún
requiere el paso 15. No hay polvo/audio ni integración nueva con cámara real.

`Assets/Acting/` contiene FBX/JSON, RenderTextures y materiales de panel nuevos.
Se reutilizan materiales del aspecto demacrado R02 y el reproductor `RigPreview`.
`Assets/Editor/ActingBuild.cs` configura únicamente escenas nuevas y verifica
escenas ya guardadas sin sobrescribirlas. El shader, las mallas y el render de
la ventana funcionan en el Editor; esto no valida brillo ni rendimiento móvil.

```bash
bash scripts/verify_acting.sh
```

La prueba genera otra carpeta con fecha, compara deformaciones y cámara contra
Blender a 30 Hz mediante Playables, verifica máscaras 960 × 540 de criatura
oculta 15–20 s y oveja oculta 21,2–25 s, y comprueba independencia de cámara
exterior. Ejecuta también `TrackingChecks.Run` (32 aserciones existentes).
La aceptación de ocultación exige cero píxeles, sin tolerancia visual añadida.

Para generar otro hito, exportar primero desde Blender con un sufijo nuevo,
copiar FBX/JSON a `Assets/Acting/` y ejecutar `ActingBuild.Configure` con
`CHUPA_ACTING` apuntando a ese nombre. La misma variable controla `Verify`
y `CaptureVideo`; `CHUPA_ACTING_EVIDENCE` elige una carpeta de salida nueva.
Invocar la ruta exacta `/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity`.
No cambiar versión, instalar paquetes ni sobreescribir hitos históricos.

`ActingBuild.SearchFall` es un diagnóstico de offsets sobre máscaras importadas.
No guarda poses ni modifica criterios; cualquier ajuste elegido se aplica en
Blender y se exporta nuevamente. La búsqueda sobre R06 y sus fallos están en
`docs/evidencias/actuacion/14_ataque_r06/`. Los derivados fallidos R01–R06 se
conservan localmente excluidos de Git. Sus informes/capturas sí se versionan.
Las secuencias PNG de vídeo también se excluyen por ser regenerables.

Las evidencias de entrega están en `docs/evidencias/actuacion/13_calma_r07/`
y `14_ataque_r07/`; los MP4 en `previews/` de la raíz Blender. Consultar
[detalle de autoría](/home/cacawatin/code/blender/chupacabras/docs/actuacion_13_14.md)
y [estado general](/home/cacawatin/code/blender/chupacabras/docs/estado.md).

No se necesita una APK nueva para la aceptación de estos dos pasos del plan,
que pide previews importados. Se conserva la APK 0.0.15, el marcador carta y
la guía `docs/pasos11_12_demacrado.md` para M#[6], aplazada. No se instaló ni
se retiró ninguna app del teléfono durante esta sesión. La aceptación física
histórica de M#[4] no se extiende a esta malla ni a la actuación nueva.
