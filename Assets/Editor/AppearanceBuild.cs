using System;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class AppearanceBuild {
  public const string Scene = "Assets/Scenes/12_AppearanceAR.unity";
  public const string Folder = "Assets/Appearance12";
  public static string Evidence =>
      Environment.GetEnvironmentVariable("CHUPA_APPEARANCE_EVIDENCE") ??
      "docs/evidencias/2026-09-30_visual";
  static void Layer(GameObject go, int layer) {
    foreach (var t in go.GetComponentsInChildren<Transform>(true))
      t.gameObject.layer = layer;
  }
  static GameObject Instance(string path, Transform parent) {
    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
    ExchangeBuild.Require(asset != null, "Missing " + path);
    var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
    go.transform.SetParent(parent, false);
    foreach (var p in go.GetComponentsInChildren<RigPreview>())
      UnityEngine.Object.DestroyImmediate(p);
    foreach (var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>()) {
      skin.updateWhenOffscreen = true;
      skin.forceMatrixRecalculationPerRender = true;
    }
    return go;
  }
  static Material MakeMaterial(string name, Color color, bool unlit = false) {
    var mat =
        new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit"
                                       : "Universal Render Pipeline/Lit"));
    mat.name = name;
    mat.SetColor("_BaseColor", color);
    mat.SetFloat("_Smoothness", .08f);
    AssetDatabase.CreateAsset(mat, Folder + "/" + name + ".mat");
    return mat;
  }
  static void Dress(GameObject go) {
    foreach (var r in go.GetComponentsInChildren<Renderer>(true)) {
      var materials = r.sharedMaterials;
      for (int i = 0; i < materials.Length; i++) {
        string name = r.name.StartsWith("Sheep") ? (i == 0 ? "Wool" : "Hoof")
                      : r.name.Contains("Eye")   ? "Eye"
                      : r.name.Contains("Teeth") || r.name.Contains("Ivory")
                          ? "Ivory"
                      : r.name.Contains("Mouth") ? "Mouth"
                      : r.name.Contains("Ridge") ? "Ridge"
                      : r.name.Contains("Crest") || r.name.Contains("Ear") ||
                              r.name.Contains("Horn")
                          ? "Horn"
                          : "Skin";
        materials[i] = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/" +
                                                               name + ".mat");
      }
      r.sharedMaterials = materials;
    }
  }
  public static void Configure() {
    TrackingBuild.RequireVersion();
    ExchangeBuild.Require(!File.Exists(Scene) && !Directory.Exists(Folder),
                          "Preserve existing appearance delivery");
    Directory.CreateDirectory(Folder);
    AssetDatabase.Refresh();
    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    MakeMaterial("Skin", new Color(.24f, .29f, .27f));
    MakeMaterial("Ridge", new Color(.34f, .37f, .30f));
    MakeMaterial("Horn", new Color(.17f, .19f, .19f));
    MakeMaterial("Ivory", new Color(.72f, .66f, .45f));
    MakeMaterial("Mouth", new Color(.045f, .029f, .033f));
    MakeMaterial("Eye", new Color(1, .73f, .045f), true);
    MakeMaterial("Wool", new Color(.8f, .76f, .64f));
    MakeMaterial("Hoof", new Color(.10f, .075f, .05f));
    var panelMat = MakeMaterial("Panel", Color.white, true);
    var pipeline = UnityEngine.Object.Instantiate(
        AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
            "Assets/Settings/MobileURP.asset"));
    pipeline.name = "AppearanceURP";
    pipeline.mainLightShadowmapResolution = 1024;
    pipeline.shadowDistance = 24;
    pipeline.shadowDepthBias = .25f;
    pipeline.shadowNormalBias = .25f;
    pipeline.msaaSampleCount = 1;
    pipeline.supportsHDR = false;
    AssetDatabase.CreateAsset(pipeline, Folder + "/AppearanceURP.asset");
    var study =
        new GameObject("AppearanceStudy").AddComponent<AppearanceStudy>();
    study.pipeline = pipeline;
    study.visualRoot = new GameObject("FigureAndPanel").transform;
    study.visualRoot.SetParent(study.transform, false);
    var figure =
        Instance("Assets/Creature/09_chupacabras_acabado/Chupacabras.prefab",
                 study.visualRoot);
    figure.name = "StaticFigure";
    foreach (var a in figure.GetComponentsInChildren<Animator>())
      UnityEngine.Object.DestroyImmediate(a);
    Layer(figure, 0);
    Dress(figure);
    figure.transform.localScale = Vector3.one * .045f;
    figure.transform.localRotation =
        Quaternion.Euler(0, 155, 0) * figure.transform.localRotation;
    figure.transform.localPosition = new Vector3(-.22f, -.08f, -.03f);
    var fillExterior = new GameObject("FigureFill").AddComponent<Light>();
    fillExterior.transform.SetParent(study.visualRoot, false);
    fillExterior.transform.localPosition = new Vector3(-.2f, .15f, -.3f);
    fillExterior.type = LightType.Point;
    fillExterior.intensity = .045f;
    fillExterior.range = 1;
    fillExterior.cullingMask = 1;
    var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
    panel.name = "CinemaPanel_240x135mm";
    UnityEngine.Object.DestroyImmediate(panel.GetComponent<Collider>());
    panel.transform.SetParent(study.visualRoot, false);
    panel.transform.localPosition = new Vector3(.25f, 0, -.001f);
    panel.transform.localScale = new Vector3(.24f, .135f, 1);
    study.panel = panel.GetComponent<Renderer>();
    study.panel.sharedMaterial = panelMat;
    study.cinemaRoot = new GameObject("CinemaOnly").transform;
    study.cinemaRoot.SetParent(study.transform, false);
    study.cinemaRoot.localPosition = new Vector3(100, 0, 0);
    var env = Instance("Assets/Environment/06_escenario.fbx", study.cinemaRoot);
    foreach (var r in env.GetComponentsInChildren<Renderer>()) {
      var source = AssetDatabase.LoadAssetAtPath<Material>(
          "Assets/Environment/" + r.name + ".mat");
      var mat = new Material(source);
      mat.name = r.name;
      // Keep the moon unlit and the set matte; brighten the ground/wood for the
      // small panel.
      if (r.name != "Env_Moon")
        mat.SetColor("_BaseColor", source.GetColor("_BaseColor") * 1.3f);
      AssetDatabase.CreateAsset(mat, Folder + "/" + r.name + ".mat");
      r.sharedMaterial = mat;
    }
    study.calm = new GameObject("CalmStudy");
    study.calm.transform.SetParent(study.cinemaRoot, false);
    var sheep =
        Instance("Assets/Rigs/10_rig_oveja.prefab", study.calm.transform);
    study.sheepAnimator = sheep.GetComponent<Animator>();
    study.sheepClip = ExchangeBuild.Clip("Assets/Rigs/10_rig_oveja.fbx");
    Dress(sheep);
    var creature =
        Instance("Assets/Creature/09_chupacabras_acabado/Chupacabras.prefab",
                 study.calm.transform);
    creature.transform.localPosition = new Vector3(-2.3f, 0, 1);
    creature.transform.localRotation =
        Quaternion.Euler(0, 160, 0) * creature.transform.localRotation;
    Dress(creature);
    study.contact =
        Instance("Assets/Rigs/11_rigs_contacto_r03.prefab", study.cinemaRoot);
    study.contactAnimator = study.contact.GetComponent<Animator>();
    study.contactClip =
        ExchangeBuild.Clip("Assets/Rigs/11_rigs_contacto_r03.fbx");
    Dress(study.contact);
    Layer(study.cinemaRoot.gameObject, 8);
    study.cinema = new GameObject("CinemaCamera").AddComponent<Camera>();
    study.cinema.transform.SetParent(study.cinemaRoot, false);
    study.cinema.cullingMask = 1 << 8;
    study.cinema.clearFlags = CameraClearFlags.SolidColor;
    study.cinema.backgroundColor = new Color(.035f, .045f, .09f);
    study.cinema.fieldOfView = 38;
    study.cinema.nearClipPlane = .05f;
    study.cinema.farClipPlane = 80;
    study.cinema.aspect = 16f / 9;
    study.cinema.depth = -1;
    study.cinema.GetUniversalAdditionalCameraData();
    var key = new GameObject("MoonKey").AddComponent<Light>();
    key.transform.SetParent(study.transform, false);
    key.type = LightType.Directional;
    key.intensity = 1.25f;
    key.color = new Color(.72f, .81f, 1);
    key.transform.rotation = Quaternion.Euler(40, 145, 0);
    key.shadows = LightShadows.Hard;
    key.shadowStrength = .7f;
    var fill = new GameObject("CinemaFill").AddComponent<Light>();
    fill.transform.SetParent(study.cinemaRoot, false);
    fill.type = LightType.Point;
    fill.intensity = 4;
    fill.range = 16;
    fill.color = new Color(.65f, .72f, 1);
    fill.transform.localPosition = new Vector3(3, 4, 4);
    fill.cullingMask = 1 << 8;
    fill.shadows = LightShadows.None;
    RenderSettings.ambientMode = AmbientMode.Flat;
    RenderSettings.ambientLight = new Color(.23f, .27f, .36f);
    RenderSettings.skybox = null;
    PrefabUtility.SaveAsPrefabAsset(study.gameObject,
                                    Folder + "/AppearanceStudy.prefab");
    var probe = new GameObject("TrackingProbe").AddComponent<TrackingProbe>();
    probe.appearance = study;
    probe.unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
    probe.litShader = Shader.Find("Universal Render Pipeline/Lit");
    AssetDatabase.SaveAssets();
    EditorSceneManager.SaveScene(study.gameObject.scene, Scene);
    Debug.Log("CHUPACABRAS_APPEARANCE_CONFIGURED");
  }
  public static void Verify() {
    TrackingBuild.RequireVersion();
    Directory.CreateDirectory(Evidence);
    EditorSceneManager.OpenScene(Scene);
    TrackingChecks.Run();
    var study = UnityEngine.Object.FindFirstObjectByType<AppearanceStudy>();
    var anchor = new GameObject("SyntheticAnchor").transform;
    var previousPipeline = QualitySettings.renderPipeline;
    try {
      QualitySettings.renderPipeline = study.pipeline;
      study.Attach(anchor);
      var view = new GameObject("ExteriorVerification").AddComponent<Camera>();
      view.cullingMask = 1;
      view.clearFlags = CameraClearFlags.SolidColor;
      view.backgroundColor = new Color(.25f, .25f, .25f);
      view.nearClipPlane = .01f;
      view.farClipPlane = 10;
      view.fieldOfView = 48;
      view.GetUniversalAdditionalCameraData();
      var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
      marker.name = "ReferenceMarker_ONLY_IN_EDITOR";
      marker.transform.position = new Vector3(0, 0, .001f);
      marker.transform.localScale = new Vector3(.22f, .22f, 1);
      var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
      mat.SetColor("_BaseColor", Color.white);
      mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                                     "Assets/Resources/AprilTagFixture.png"));
      marker.GetComponent<Renderer>().sharedMaterial = mat;
      ExchangeBuild.Require(study.visualRoot.parent == anchor &&
                                !study.cinemaRoot.IsChildOf(anchor),
                            "Independent cinema");
      ExchangeBuild.Require(study.cinema.cullingMask == (1 << 8),
                            "Cinema layer");
      ExchangeBuild.Require(
          study.visualRoot.GetComponentsInChildren<Animator>().Length == 0,
          "Static exterior");
      ExchangeBuild.Require(
          study.visualRoot.GetComponentsInChildren<Renderer>().All(
              r => r.gameObject.layer == 0),
          "Exterior layer");
      for (int shot = 0; shot < 3; shot++) {
        study.Present(true, shot * 6, true);
        ExchangeBuild.SaveCamera(study.cinema,
                                 Evidence + "/cinema_" + shot + ".png");
      }
      study.Present(true, 6, true);
      var pos = study.cinema.transform.position;
      var rotation = study.cinema.transform.rotation;
      ExchangeBuild.SaveCamera(study.cinema,
                               Evidence + ("/internal_before." + "png"));
      foreach (var angle in new[] { ("front", new Vector3(.015f, .07f, -.85f)),
                                    ("left", new Vector3(-.24f, .12f, -.85f)),
                                    ("right",
                                     new Vector3(.27f, .12f, -.85f)) }) {
        view.transform.position = angle.Item2;
        view.transform.LookAt(new Vector3(.015f, 0, 0));
        ExchangeBuild.SaveCamera(view, Evidence + "/composition_" +
                                           angle.Item1 + ".png");
        // Project conservative bounds of every exterior mesh: the entire 220 mm
        // protected square (180 mm drawing + 20 mm margin each side) must stay
        // unobscured.
        var tagRect = new Rect();
        var tagPoints =
            new[] { new Vector3(-.11f, -.11f, 0), new Vector3(.11f, -.11f, 0),
                    new Vector3(.11f, .11f, 0), new Vector3(-.11f, .11f, 0) }
                .Select(p => view.WorldToViewportPoint(p))
                .ToArray();
        tagRect =
            Rect.MinMaxRect(tagPoints.Min(p => p.x), tagPoints.Min(p => p.y),
                            tagPoints.Max(p => p.x), tagPoints.Max(p => p.y));
        foreach (var mesh in study.visualRoot
                     .GetComponentsInChildren<MeshFilter>()) {
          var points = mesh.sharedMesh.vertices
                           .Select(v => view.WorldToViewportPoint(
                                       mesh.transform.TransformPoint(v)))
                           .ToArray();
          var rect =
              Rect.MinMaxRect(points.Min(p => p.x), points.Min(p => p.y),
                              points.Max(p => p.x), points.Max(p => p.y));
          ExchangeBuild.Require(!tagRect.Overlaps(rect),
                                "Protected marker overlaps " + mesh.name +
                                    " from " + angle.Item1);
        }
      }
      anchor.SetPositionAndRotation(new Vector3(.02f, -.01f, .02f),
                                    Quaternion.Euler(5, 10, 3));
      ExchangeBuild.Require(study.cinema.transform.position == pos &&
                                study.cinema.transform.rotation == rotation,
                            "Anchor moved cinema");
      ExchangeBuild.SaveCamera(study.cinema,
                               Evidence + ("/internal_after." + "png"));
      ExchangeBuild.Require(
          File.ReadAllBytes(Evidence + "/internal_before.png")
              .SequenceEqual(
                  File.ReadAllBytes(Evidence + "/internal_after.png")),
          "Exterior changes cinema pixels");
      study.Present(false, 6, true);
      ExchangeBuild.Require(!study.cinema.enabled,
                            "Lost marker must stop render");
      File.WriteAllText(
          Evidence + "/checks.json",
          "{\"passed\":true,\"physicalDevice\":false,\"unity\":\"" +
              Application.unityVersion +
              ("\",\"shots\":3,\"externalViews\":3,\"renderTexture\":[960," +
               "540],\"panelMeters\":[0.24,0.135]," +
               "\"internalPixelsUnchanged\":true}\n"));
      Debug.Log("CHUPACABRAS_APPEARANCE_VERIFIED");
    } finally {
      QualitySettings.renderPipeline = previousPipeline;
      study.ReleaseResources();
      UnityEngine.Object.DestroyImmediate(study.gameObject);
    }
  }
  public static void BuildAndroid() {
    TrackingBuild.RequireVersion();
    string product = PlayerSettings.productName,
           version = PlayerSettings.bundleVersion;
    int code = PlayerSettings.Android.bundleVersionCode;
    string id =
        PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
    var pipeline = GraphicsSettings.defaultRenderPipeline;
    try {
      PlayerSettings.productName = "Chupacabras — Aspecto 12";
      PlayerSettings.bundleVersion = "0.0.12";
      PlayerSettings.Android.bundleVersionCode = 12;
      PlayerSettings.SetApplicationIdentifier(
          NamedBuildTarget.Android, "com.chupacabras.ar.appearance12");
      GraphicsSettings.defaultRenderPipeline =
          AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
              Folder + "/AppearanceURP.asset");
      string output = "builds/android/12_appearance_" +
                      DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".apk";
      ExchangeBuild.Require(!File.Exists(output), "Preserve APK");
      var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
        scenes = new[] { Scene }, locationPathName = output,
        target = BuildTarget.Android, options = BuildOptions.Development
      });
      File.WriteAllText(
          output + ".build.txt",
          $"Unity: {Application.unityVersion}\nResult: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\nDuration: {report.summary.totalTime}\nPhysical G20: pending M#[4]\n");
      if (report.summary.result != BuildResult.Succeeded)
        throw new BuildFailedException("Appearance APK failed");
      Debug.Log("CHUPACABRAS_APPEARANCE_APK " + output);
    } finally {
      PlayerSettings.productName = product;
      PlayerSettings.bundleVersion = version;
      PlayerSettings.Android.bundleVersionCode = code;
      PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, id);
      GraphicsSettings.defaultRenderPipeline = pipeline;
      AssetDatabase.SaveAssets();
    }
  }
}
