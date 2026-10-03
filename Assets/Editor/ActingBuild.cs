using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Independent steps 13/14 previews; production Timeline and AR integration
// remain steps 17/20. Reuse the existing clip player and appearance resources.
public static class ActingBuild {
  static string Name =>
      Environment.GetEnvironmentVariable("CHUPA_ACTING") ?? "14_ataque_r07";
  static string Model => "Assets/Acting/" + Name + ".fbx";
  static string Scene => "Assets/Scenes/" + Name + ".unity";
  static string Evidence =>
      Environment.GetEnvironmentVariable("CHUPA_ACTING_EVIDENCE") ??
      "docs/evidencias/actuacion/" + Name;
  const string Appearance = "Assets/Appearance12DemacradoR02/";
  [Serializable]
  public class Sample {
    public float time;
    public Vector3 camera, forward, creature;
    public RigBuild.MeshSample[] meshes;
  }
  [Serializable]
  public class Reference {
    public float duration;
    public Sample[] samples;
  }
  [Serializable]
  public class Result {
    public bool passed, physicalDevice = false;
    public float duration, maxCameraError, maxDirectionError, maxVertexError,
        maxCameraStep, maxCreatureStep;
    public int samples, verticesCompared, eyesBefore, sheepBefore, sheepControl;
    public int[] hiddenCreaturePixels, hiddenSheepPixels;
    public string[] failures;
  }
  static Reference Read() => JsonUtility.FromJson<Reference>(
      File.ReadAllText("Assets/Acting/" + Name + ".json"));
  public static void Configure() {
    TrackingBuild.RequireVersion();
    ExchangeBuild.Require(!File.Exists(Scene), "Preserve scene " + Scene);
    ExchangeBuild.ConfigureModel(Model);
    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    var root = (GameObject)PrefabUtility.InstantiatePrefab(
        AssetDatabase.LoadAssetAtPath<GameObject>(Model));
    foreach (var t in root.GetComponentsInChildren<Transform>())
      t.gameObject.layer = 8;
    foreach (var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>()) {
      skin.updateWhenOffscreen = true;
      skin.forceMatrixRecalculationPerRender = true;
      skin.quality = SkinQuality.Bone4;
    }
    foreach (var r in root.GetComponentsInChildren<Renderer>()) {
      var mats = r.sharedMaterials;
      for (int i = 0; i < mats.Length; i++) {
        string n = r.name;
        string m =
            n.StartsWith("Env_")    ? n
            : n.StartsWith("Sheep") ? (i == 0 ? "Wool" : "Hoof")
            : n.Contains("Sockets") || n.Contains("Mouth") ? "Mouth"
            : n.Contains("Eye")                            ? "Eye"
            : n.Contains("Ivory") || n.Contains("Teeth")   ? "Ivory"
            : n.Contains("Ridge")                          ? "Ridge"
            : n.Contains("Crest") || n.Contains("Ear") || n.Contains("Horn")
                ? "Horn"
                : "Skin";
        mats[i] =
            AssetDatabase.LoadAssetAtPath<Material>(Appearance + m + ".mat");
        ExchangeBuild.Require(mats[i] != null, "Missing material " + m);
      }
      r.sharedMaterials = mats;
    }
    var anim = root.GetComponent<Animator>();
    if (anim == null)
      anim = root.AddComponent<Animator>();
    anim.applyRootMotion = false;
    anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    var player = root.AddComponent<RigPreview>();
    player.animator = anim;
    player.clip = ExchangeBuild.Clip(Model);
    var cam = ExchangeBuild.Find(root, "CinemaCamera").GetComponent<Camera>();
    cam.cullingMask = 1 << 8;
    cam.clearFlags = CameraClearFlags.SolidColor;
    cam.backgroundColor = new Color(.035f, .045f, .09f);
    cam.nearClipPlane = .05f;
    cam.farClipPlane = 150;
    cam.aspect = 16f / 9;
    cam.gateFit = Camera.GateFitMode.Horizontal;
    cam.GetUniversalAdditionalCameraData();
    var rt = new RenderTexture(960, 540, 24) { name = Name + "_CinemaRT" };
    AssetDatabase.CreateAsset(rt,
                              "Assets/Acting/" + rt.name + ".renderTexture");
    cam.targetTexture = rt;
    cam.depth = -1;
    var anchor = new GameObject("ARAnchorReference");
    var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
    UnityEngine.Object.DestroyImmediate(panel.GetComponent<Collider>());
    panel.name = "CinemaPanel_240x135mm";
    panel.transform.SetParent(anchor.transform, false);
    panel.transform.localScale = new Vector3(.24f, .135f, 1);
    var panelMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    panelMat.SetTexture("_BaseMap", rt);
    AssetDatabase.CreateAsset(panelMat, "Assets/Acting/" + Name + "_Panel.mat");
    panel.GetComponent<Renderer>().sharedMaterial = panelMat;
    var exterior =
        new GameObject("ExteriorPreviewCamera").AddComponent<Camera>();
    exterior.transform.position = new Vector3(0, 0, -.25f);
    exterior.transform.LookAt(Vector3.zero);
    exterior.cullingMask = 1;
    exterior.clearFlags = CameraClearFlags.SolidColor;
    exterior.backgroundColor = new Color(.07f, .08f, .09f);
    exterior.nearClipPlane = .01f;
    var light = new GameObject("CinemaMoonKey").AddComponent<Light>();
    light.type = LightType.Directional;
    light.intensity = 1.3f;
    light.color = new Color(.76f, .84f, 1);
    light.cullingMask = 1 << 8;
    light.transform.rotation = Quaternion.Euler(42, -30, 0);
    RenderSettings.ambientMode = AmbientMode.Flat;
    RenderSettings.ambientLight = new Color(.24f, .27f, .34f);
    player.clip.SampleAnimation(root, 0);
    EditorSceneManager.SaveScene(root.scene, Scene);
    AssetDatabase.SaveAssets();
    Debug.Log("ACTING_CONFIGURE_OK " + Name);
  }
  public static void Verify() {
    TrackingBuild.RequireVersion();
    EditorSceneManager.OpenScene(Scene);
    Directory.CreateDirectory(Evidence);
    var player = UnityEngine.Object.FindFirstObjectByType<RigPreview>();
    var root = player.gameObject;
    var cam = ExchangeBuild.Find(root, "CinemaCamera").GetComponent<Camera>();
    var graph = PlayableGraph.Create("Acting import check");
    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    var playable = AnimationClipPlayable.Create(graph, player.clip);
    playable.SetApplyFootIK(false);
    AnimationPlayableOutput.Create(graph, "Acting", player.animator)
        .SetSourcePlayable(playable);
    graph.Play();
    void Evaluate(double t) {
      playable.SetTime(t);
      graph.Evaluate(0);
    }
    var reference = Read();
    var result = new Result { duration = player.clip.length };
    var failures = new List<string>();
    void Check(bool ok, string message) {
      if (!ok && failures.Count < 25)
        failures.Add(message);
    }
    Check(Mathf.Abs(result.duration - reference.duration) < .0001f,
          "Clip duration");
    var skins =
        root.GetComponentsInChildren<SkinnedMeshRenderer>().ToDictionary(
            s => s.name);
    var mesh = new Mesh();
    var indices = new Dictionary<string, int[]>();
    Vector3 lastCamera = Vector3.zero, lastCreature = Vector3.zero;
    foreach (var s in reference.samples) {
      Evaluate(s.time);
      result.maxCameraError =
          Mathf.Max(result.maxCameraError,
                    Vector3.Distance(cam.transform.position,
                                     ExchangeBuild.Convert(s.camera)));
      result.maxDirectionError =
          Mathf.Max(result.maxDirectionError,
                    Vector3.Distance(cam.transform.forward,
                                     ExchangeBuild.Convert(s.forward)));
      var pos = ExchangeBuild.Find(root, "ChupacabrasAssetRoot").position;
      if (result.samples > 0) {
        result.maxCameraStep =
            Mathf.Max(result.maxCameraStep,
                      Vector3.Distance(lastCamera, cam.transform.position));
        result.maxCreatureStep = Mathf.Max(result.maxCreatureStep,
                                           Vector3.Distance(lastCreature, pos));
      }
      lastCamera = cam.transform.position;
      lastCreature = pos;
      foreach (var m in s.meshes) {
        var skin = skins[m.name];
        skin.BakeMesh(mesh, true);
        var points = mesh.vertices.Select(v => skin.transform.TransformPoint(v))
                         .ToArray();
        // FBX splits vertices at UV/material seams. Match indices once in the
        // initial pose, then compare the SAME vertices throughout deformation.
        if (!indices.ContainsKey(m.name))
          indices[m.name] =
              m.vertices
                  .Select(e => Enumerable.Range(0, points.Length)
                                   .OrderBy(i => (points[i] -
                                                  ExchangeBuild.Convert(e))
                                                     .sqrMagnitude)
                                   .First())
                  .ToArray();
        for (int i = 0; i < m.vertices.Length; i++) {
          result.maxVertexError =
              Mathf.Max(result.maxVertexError,
                        Vector3.Distance(points[indices[m.name][i]],
                                         ExchangeBuild.Convert(m.vertices[i])));
          result.verticesCompared++;
        }
      }
      result.samples++;
      if (result.samples % 150 == 0)
        Debug.Log("ACTING_SAMPLES " + result.samples);
    }
    Check(result.maxVertexError < .001f,
          "Imported deformation " + result.maxVertexError);
    Check(result.maxCameraError < .001f && result.maxDirectionError < .001f,
          "Camera export");
    Check(result.maxCameraStep < .26f, "Camera discontinuity");
    Check(result.maxCreatureStep < .9f, "Creature discontinuity");
    var renderers = root.GetComponentsInChildren<Renderer>();
    var saved = renderers.Select(r => r.sharedMaterials).ToArray();
    var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    black.SetColor("_BaseColor", Color.black);
    var white = new Material(black);
    white.SetColor("_BaseColor", Color.white);
    var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
    var oldBG = cam.backgroundColor;
    cam.backgroundColor = Color.black;
    void Mask(Func<Renderer, bool> predicate) {
      foreach (var r in renderers)
        r.sharedMaterials = r.sharedMaterials
                                .Select(
                                    _ => predicate(r) ? white : black)
                                .ToArray();
    }
    int Count(double t) {
      Evaluate(t);
      cam.Render();
      var old = RenderTexture.active;
      RenderTexture.active = cam.targetTexture;
      image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
      image.Apply();
      RenderTexture.active = old;
      return image.GetPixels32().Count(p =>
                                           p.r > 200 && p.g > 200 && p.b > 200);
    }
    Mask(r => r.name.StartsWith("Chupa_Eye"));
    result.eyesBefore = Count(10);
    Check(result.eyesBefore > 0, "Eyes absent before withdrawal");
    Mask(r => r.name.StartsWith("Chupa_"));
    result.hiddenCreaturePixels =
        Enumerable.Range(450, 150).Select(f => Count(f / 30.0)).ToArray();
    Check(result.hiddenCreaturePixels.All(n => n == 0),
          "Creature/eyes visible at 15–20 s");
    Mask(r => r.name.StartsWith("Sheep_"));
    result.sheepBefore = Count(10);
    Check(result.sheepBefore > 100, "Sheep not legible in calm");
    if (reference.duration > 20) {
      result.hiddenSheepPixels =
          Enumerable.Range(636, 115)
              .Select(f => {
                int count = Count(f / 30.0);
                if (count > 0)
                  File.WriteAllBytes(Evidence + "/failed_mask_" + f + ".png",
                                     image.EncodeToPNG());
                return count;
              })
              .ToArray();
      Check(result.hiddenSheepPixels.All(n => n == 0),
            "Sheep visible from landing through 25 s");
      foreach (var r in renderers.Where(r => r.name.StartsWith("Chupa_")))
        r.enabled = false;
      result.sheepControl = Count(23);
      Check(result.sheepControl > 100,
            "Invalid occlusion: sheep disabled/outside view");
      foreach (var r in renderers.Where(r => r.name.StartsWith("Chupa_")))
        r.enabled = true;
    }
    for (int i = 0; i < renderers.Length; i++)
      renderers[i].sharedMaterials = saved[i];
    cam.backgroundColor = oldBG;
    foreach (float t in new[] { 0, 5, 10, 14, 15, 18.5f, 19.5f, 20, 20.2f,
                                20.5f, 20.8f, 21.2f, 21.4f, 22, 23, 25 }) {
      if (t > reference.duration)
        continue;
      Evaluate(t);
      ExchangeBuild.SaveCamera(
          cam,
          Evidence + "/frame_" +
              t.ToString("00.0",
                         System.Globalization.CultureInfo.InvariantCulture) +
              ".png");
    }
    Evaluate(reference.duration > 20 ? 23 : 10);
    ExchangeBuild.SaveCamera(cam, Evidence + "/internal_before.png");
    var anchor = GameObject.Find("ARAnchorReference").transform;
    anchor.SetPositionAndRotation(new Vector3(.04f, .01f, .03f),
                                  Quaternion.Euler(4, 20, 3));
    var exterior =
        GameObject.Find("ExteriorPreviewCamera").GetComponent<Camera>();
    exterior.transform.position += new Vector3(.08f, .03f, -.04f);
    exterior.transform.LookAt(anchor);
    ExchangeBuild.SaveCamera(cam, Evidence + "/internal_after.png");
    Check(File.ReadAllBytes(Evidence + "/internal_before.png")
              .SequenceEqual(
                  File.ReadAllBytes(Evidence + "/internal_after.png")),
          "AR reference altered internal image");
    cam.Render();
    ExchangeBuild.SaveCamera(exterior, Evidence + "/window.png");
    result.failures = failures.ToArray();
    result.passed = failures.Count == 0;
    File.WriteAllText(Evidence + "/checks.json",
                      JsonUtility.ToJson(result, true) + "\n");
    graph.Destroy();
    TrackingChecks.Run();
    ExchangeBuild.Require(result.passed, string.Join("; ", failures));
    Debug.Log("ACTING_VERIFY_OK " + Name);
  }
  public static void CaptureVideo() {
    TrackingBuild.RequireVersion();
    EditorSceneManager.OpenScene(Scene);
    var player = UnityEngine.Object.FindFirstObjectByType<RigPreview>();
    var cam = ExchangeBuild.Find(player.gameObject, "CinemaCamera")
                  .GetComponent<Camera>();
    string folder = Evidence + "/frames";
    ExchangeBuild.Require(!Directory.Exists(folder), "Preserve frames");
    Directory.CreateDirectory(folder);
    var graph = PlayableGraph.Create("Acting video");
    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    var clip = AnimationClipPlayable.Create(graph, player.clip);
    AnimationPlayableOutput.Create(graph, "Acting", player.animator)
        .SetSourcePlayable(clip);
    graph.Play();
    for (int f = 0; f < Mathf.RoundToInt(Read().duration * 15); f++) {
      clip.SetTime(f / 15.0);
      graph.Evaluate(0);
      ExchangeBuild.SaveCamera(cam, folder + "/" + f.ToString("0000") + ".png");
    }
    graph.Destroy();
    Debug.Log("ACTING_VIDEO_OK " + Name);
  }
  // Diagnose small fall offsets on the actual imported masks. Does not save
  // poses or relax acceptance; apply the chosen motion in Blender and
  // re-export.
  public static void SearchFall() {
    TrackingBuild.RequireVersion();
    EditorSceneManager.OpenScene(Scene);
    Directory.CreateDirectory(Evidence);
    var player = UnityEngine.Object.FindFirstObjectByType<RigPreview>();
    var cam = ExchangeBuild.Find(player.gameObject, "CinemaCamera")
                  .GetComponent<Camera>();
    var sheep = ExchangeBuild.Find(player.gameObject, "SheepAssetRoot");
    var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    black.SetColor("_BaseColor", Color.black);
    var white = new Material(black);
    white.SetColor("_BaseColor", Color.white);
    foreach (var r in player.GetComponentsInChildren<Renderer>())
      r.sharedMaterials =
          r.sharedMaterials
              .Select(
                  _ => r.name.StartsWith("Sheep_") ? white : black)
              .ToArray();
    cam.backgroundColor = Color.black;
    var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
    var graph = PlayableGraph.Create("Fall diagnostic");
    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    var clip = AnimationClipPlayable.Create(graph, player.clip);
    AnimationPlayableOutput.Create(graph, "Fall", player.animator)
        .SetSourcePlayable(clip);
    graph.Play();
    var rows = new List<string> {
      "delta_x_coefficient,delta_y_coefficient,total_pixels,max_pixels"
    };
    foreach (float dx in new[] { 0f, .08f, .18f, .28f })
      foreach (float dy in new[] { 0f, .10f, .20f, .30f }) {
        int total = 0, max = 0;
        for (int f = 640; f <= 653; f++) {
          double t = f / 30.0;
          clip.SetTime(t);
          graph.Evaluate(0);
          float u = Mathf.Clamp01(((float)t - 21.2f) / .5f);
          float fall = u * u * u * (10 + u * (-15 + 6 * u));
          sheep.position += new Vector3(dx, 0, dy) * Mathf.Sin(Mathf.PI * fall);
          cam.Render();
          var previous = RenderTexture.active;
          RenderTexture.active = cam.targetTexture;
          image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
          image.Apply();
          RenderTexture.active = previous;
          int count = image.GetPixels32().Count(p => p.r > 200 && p.g > 200 &&
                                                     p.b > 200);
          total += count;
          max = Mathf.Max(max, count);
        }
        rows.Add($"{dx},{dy},{total},{max}");
      }
    graph.Destroy();
    File.WriteAllLines(Evidence + "/fall_search.csv", rows);
    Debug.Log("FALL_SEARCH_OK");
  }
}
