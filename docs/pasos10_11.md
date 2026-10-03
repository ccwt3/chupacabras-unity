> **Referencia histórica anterior al cambio de alcance del 2 de octubre de 2026.**
> Resultados y entregas se conservan; sus próximos pasos, ventana/corto y
> solicitudes manuales no son instrucciones vigentes. Ver [estado](estado.md)
> y [migración a figura AR](figura_ar.md). M#[6] no se declara aprobada.

# Pasos 10–11: rigs e intercambio

Fuentes Blender y generadores en `/home/cacawatin/code/blender/chupacabras`.
Proyecto AR en esta raíz; exclusivamente Unity 6000.3.22f1 desde Linux.
Documentación general: `docs/rigs_contacto.md` y `docs/estado.md` de Blender.

Entregas vigentes:

- `Assets/Scenes/10_rig_oveja.unity`: 8 s de poses de oveja; 24 huesos y
  `WoolCompression`, 66 vértices afectados, pastoreo/forcejeo y apoyo.
- `Assets/Scenes/11_rig_chupacabras_poses.unity`: 6 s de pruebas FK de patas,
  columna, cola y mandíbula; rig original de 23 huesos.
- `Assets/Scenes/11_rigs_contacto_r03.unity`: 6 s de agarre establecido,
  cuello comprimido, pataleo y desplazamiento conjunto. La malla de lana
  usa triangulación fija idéntica en ambos programas.

FBX/JSON/materiales/prefabs en `Assets/Rigs/`. `RigPreview` usa la duración del
clip y conserva la independencia de la raíz exterior. La producción de 40 s,
las demos y los sistemas AprilTag/cámara existentes no cambian. Estas escenas
son pruebas de recursos, sin APK ni aceptación física nueva. La actuación y
la sincronización final de pisadas corresponden a los pasos 13–15; la
iluminación/piso de revisión no completan el paso 12.

`RigBuild.Configure` crea una escena nueva (rechaza una existente);
`RigBuild.Verify` compara vértices evaluados, curva de shape, duración, apoyo,
contacto sobre los triángulos de lana y raíz exterior transformada. Para repetir
las tres aceptaciones con capturas nuevas:

```bash
bash scripts/verify_rigs.sh
```

El primer intento tenía contacto correcto entre puntos pero discrepancia de
5,31 mm contra las superficies por triangulación. R02 resolvió la superficie,
pero triangulación en modo edición descartó una cara del recurso reutilizado.
**R03 conserva los 612 triángulos mediante sus índices de loop originales**,
con pesos, UV, materiales y shape key. Mantener este orden de operaciones en
la producción. Primer intento/R02 y reportes quedan como diagnóstico.

No incluir en el commit los cambios preexistentes de GraphicsSettings,
QualitySettings ni el archivo local PackageManagerSettings. No se modificó
la configuración global ni se usó otra instalación Unity.

Resultados finales: oveja 241 cuadros/error máximo 0,04261 mm; controles
181 cuadros/0,00300 mm; contacto R03 181 cuadros/0,02770 mm geométricos
y 0,001408 mm contra superficie. 11.162 triángulos conjuntos conservados.
Regresiones de escenario/oveja y 32 aserciones de tracking correctas.
533 archivos históricos Blender intactos. Capturas y checks en
`docs/evidencias/2026-09-24_rigs/`; logs comprimidos `.log.gz`. Logs crudos
de regresión archivados en `Library/Session_20260924_rigs_logs/`.
La oveja conserva el solapamiento de caras heredado, sin bordes abiertos;
no se declara manifold. Próximos pasos 12–13; M#[4] solo tras preparar APK.
