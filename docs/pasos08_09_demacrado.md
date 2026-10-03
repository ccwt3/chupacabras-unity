> **Referencia histórica anterior al cambio de alcance del 2 de octubre de 2026.**
> Resultados y entregas se conservan; sus próximos pasos, ventana/corto y
> solicitudes manuales no son instrucciones vigentes. Ver [estado](estado.md)
> y [migración a figura AR](figura_ar.md). M#[6] no se declara aprobada.

# Revisión demacrada — pasos 8/9, 2 de octubre de 2026

El usuario pidió menos volumen muscular y aspecto más esquelético/tenebroso.
Se importan fuentes nuevas desde `/home/cacawatin/code/blender/chupacabras`;
la documentación detallada y el estado general están allí en
`docs/chupacabras_demacrado.md` y `docs/estado.md`.

Entregas actuales: `Assets/Creature/08_chupacabras_demacrado_r01/` y
`Assets/Creature/09_chupacabras_demacrado_r02/`. Incluyen fuente FBX, informe
Blender, materiales URP y prefab independiente. Las escenas de revisión llevan
el mismo nombre con `_contexto_r03.unity`. El contexto R02 de forma conserva
el fallo inicial de ocultamiento para diagnóstico, no es la entrega vigente.

Acabado: 30.880 triángulos, 18 mallas, seis materiales. Abdomen hundido,
articulaciones/costillas visibles, cuello/cola/extremidades más delgados,
rostro estrecho, cuencas oscuras y ojos amarillos recogidos. No se altera la
APK 0.0.14 ni su composición centrada sobre el tag; tampoco las demos previas.

Verificado exclusivamente en Unity **6000.3.22f1**, instalación existente desde
Linux: importación/ejes/UV/materiales, duración de contexto 40 s, 32 aserciones
de tracking; 150/114 cuadros ocultos y 30 vacíos por modelo a 320×180.
Error máximo de límites 0,0000004299 m. `LeanCameraCheck.Verify` reabre la
escena acabada y confirma ocultamiento de los 264 cuadros a 960×540;
control positivo con oveja visible si se retira el oclusor (12.809 píxeles).
La cámara corregida Blender `(0.3,-4,2.8)` es Unity `(0.3,2.8,-4)`.

Evidencias: `docs/evidencias/2026-10-02_demacrado_*`. Los logs de ejecución
completos se conservan en la raíz Blender. La primera cámara falla; los
informes `forma_r03`, `acabado` y `fullres` son las verificaciones finales.

Reapertura reproducible desde esta raíz:

```bash
CHUPA_MODEL=09_chupacabras_demacrado_r02 CHUPA_CONTEXT_SUFFIX=_contexto_r03 CHUPA_CREATURE_EVIDENCE=docs/evidencias/demacrado_reverificacion /home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity -job-worker-count 2 -batchmode -quit -projectPath "$PWD" -executeMethod CreatureBuild.Verify -logFile /tmp/demacrado_reverificacion.log
/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Unity -job-worker-count 2 -batchmode -quit -projectPath "$PWD" -executeMethod LeanCameraCheck.Verify -logFile /tmp/demacrado_fullres.log
clang-format --dry-run --Werror Assets/Editor/LeanCameraCheck.cs
```

`LeanCameraCheck.Run` conserva la búsqueda diagnóstica; no guarda cambios en
las escenas. Los informes diagnósticos se regeneran al repetir esos métodos.
No ejecutar Configure sobre escenas existentes.

**Limitaciones:** contexto de volumen con oveja rígida y cortes del bloqueo;
no es actuación final ni prueba física. El nuevo rig/contacto requiere revisión
de paso 11 y la integración de aspecto/composición paso 12. Solo después seguir
13–14. Los ojos son apenas un píxel en el plano lejano de revisión: legibilidad
móvil pendiente. No hay APK nueva ni prueba del G20/S23. M#[4] conserva la
aceptación histórica de 0.0.14, no se transfiere a esta anatomía. M#[5] sigue
prevista para 20; ninguna intervención manual requerida ahora.

Los tres cambios previamente preparados en ProjectSettings se conservan y
se excluyen del commit de esta sesión; no se modifican ajustes globales.
