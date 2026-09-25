using System;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class RigBuild {
  static string Name =>
      Environment.GetEnvironmentVariable("CHUPA_RIG") ?? "10_rig_oveja";
  static string Model => "Assets/Rigs/" + Name + ".fbx";
  static string Evidence =>
      Environment.GetEnvironmentVariable("CHUPA_RIG_EVIDENCE") ??
      "docs/evidencias/2026-09-24_rigs/" + Name;
  [Serializable]
  public class MeshSample {
    public string name;
    public Vector3[] vertices;
  }
  [Serializable]
  public class Sample {
    public float time, shape;
    public string label;
    public MeshSample[] meshes;
    public Vector3 upper, lower, neckUpper, neckLower;
  }
  [Serializable]
  public class Reference {
    public float duration;
    public Sample[] samples;
  }
  [Serializable]
  public class Report {
    public bool passed, physicalDevice = false;
    public int samples, verticesCompared, triangles;
    public float duration, maxVertexError, maxShapeError,
        minHeight = 100, maxContact, maxSurfaceContact;
  }
  static float SegmentDistance(Vector3 p, Vector3 a, Vector3 b) {
    var d = b - a;
    return Vector3.Distance(
        p, a + d * Mathf.Clamp01(Vector3.Dot(p - a, d) / d.sqrMagnitude));
  }
  static float SurfaceDistance(Vector3 p, Vector3[] vertices, int[] triangles) {
    float best = float.PositiveInfinity;
    for (int i = 0; i < triangles.Length; i += 3) {
      var a = vertices[triangles[i]];
      var b = vertices[triangles[i + 1]];
      var c = vertices[triangles[i + 2]];
      var u = b - a;
      var v = c - a;
      var w = p - a;
      float uu = Vector3.Dot(u, u), uv = Vector3.Dot(u, v),
            vv = Vector3.Dot(v, v);
      float wu = Vector3.Dot(w, u), wv = Vector3.Dot(w, v),
            den = uu * vv - uv * uv;
      float x = (wu * vv - wv * uv) / den, y = (wv * uu - wu * uv) / den;
      float distance = x >= 0 && y >= 0 && x + y <= 1
                           ? Vector3.Distance(p, a + x * u + y * v)
                           : Mathf.Min(SegmentDistance(p, a, b),
                                       Mathf.Min(SegmentDistance(p, b, c),
                                                 SegmentDistance(p, c, a)));
      best = Mathf.Min(best, distance);
    }
    return best;
  }
  public static void Configure() {
    TrackingBuild.RequireVersion();
    string scene = "Assets/Scenes/" + Name + ".unity";
    ExchangeBuild.Require(!File.Exists(scene), "Preserve previous rig scene");
    ExchangeBuild.ConfigureModel(Model);
    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    var anchor = new GameObject("ARAnchorReference");
    var go = (GameObject)PrefabUtility.InstantiatePrefab(
        AssetDatabase.LoadAssetAtPath<GameObject>(Model));
    go.transform.SetParent(anchor.transform, false);
    foreach (var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>()) {
      skin.forceMatrixRecalculationPerRender = true;
      skin.updateWhenOffscreen = true;
      skin.quality = SkinQuality.Bone4;
    }
    foreach (var renderer in go.GetComponentsInChildren<Renderer>()) {
      Material[] mats = new Material[renderer.sharedMaterials.Length];
      for (int i = 0; i < mats.Length; i++) {
        string asset =
            "Assets/Rigs/" + Name + "_" + renderer.name + "_" + i + ".mat";
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        Color color = new Color(.135f, .17f, .16f);
        string n = renderer.name;
        if (n.StartsWith("Sheep"))
          color = i == 0 ? new Color(.78f, .74f, .61f)
                         : new Color(.065f, .055f, .045f);
        if (n.Contains("Eye"))
          color = new Color(1, .64f, .045f);
        if (n.Contains("Ivory") || n.Contains("Teeth"))
          color = new Color(.59f, .56f, .4f);
        if (n.Contains("Horn") || n.Contains("Crest") || n.Contains("Ear"))
          color = new Color(.105f, .12f, .12f);
        if (n.Contains("Ridge"))
          color = new Color(.23f, .26f, .235f);
        if (n.Contains("Mouth"))
          color = new Color(.036f, .026f, .028f);
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", 0);
        AssetDatabase.CreateAsset(mat, asset);
        mats[i] = mat;
      }
      renderer.sharedMaterials = mats;
    }
    var animator = go.GetComponent<Animator>();
    if (animator == null)
      animator = go.AddComponent<Animator>();
    animator.applyRootMotion = false;
    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    var preview = go.AddComponent<RigPreview>();
    preview.animator = animator;
    preview.clip = ExchangeBuild.Clip(Model);
    var cam = new GameObject("RigReviewCamera").AddComponent<Camera>();
    cam.transform.position = Name.StartsWith("10")
                                 ? new Vector3(2.5f, 1.9f, -3.5f)
                                 : new Vector3(6, 3.5f, 6);
    cam.transform.LookAt(Name.StartsWith("10") ? new Vector3(0, .55f, -.1f)
                                               : new Vector3(-.6f, .8f, 0));
    cam.fieldOfView = 38;
    cam.nearClipPlane = .03f;
    cam.farClipPlane = 100;
    cam.clearFlags = CameraClearFlags.SolidColor;
    cam.backgroundColor = new Color(.08f, .09f, .12f);
    var light = new GameObject("ReviewLight").AddComponent<Light>();
    light.type = LightType.Directional;
    light.intensity = 1.2f;
    light.transform.rotation = Quaternion.Euler(35, -30, 0);
    RenderSettings.ambientLight = new Color(.2f, .2f, .2f);
    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    PrefabUtility.SaveAsPrefabAsset(go, "Assets/Rigs/" + Name + ".prefab");
    AssetDatabase.SaveAssets();
    EditorSceneManager.SaveScene(go.scene, scene);
    Verify();
  }
  public static void Verify() {
    TrackingBuild.RequireVersion();
    Directory.CreateDirectory(Evidence);
    EditorSceneManager.OpenScene("Assets/Scenes/" + Name + ".unity");
    var preview = UnityEngine.Object.FindFirstObjectByType<RigPreview>();
    var go = preview.gameObject;
    var clip = preview.clip;
    foreach (var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>()) {
      skin.forceMatrixRecalculationPerRender = true;
      skin.updateWhenOffscreen = true;
    }
    var reviewLight = GameObject.Find("ReviewLight").GetComponent<Light>();
    reviewLight.intensity = Name.StartsWith("10") ? 1.2f : 1.6f;
    reviewLight.transform.rotation =
        Quaternion.Euler(35, Name.StartsWith("10") ? -30 : 150, 0);
    if (GameObject.Find("ReviewFloor") == null) {
      var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
      floor.name = "ReviewFloor";
      floor.transform.localScale = Vector3.one * 2;
      var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
      mat.SetColor("_BaseColor", new Color(.11f, .12f, .14f));
      mat.SetFloat("_Smoothness", 0);
      string path = "Assets/Rigs/" + Name + "_ReviewFloor.mat";
      AssetDatabase.CreateAsset(mat, path);
      floor.GetComponent<Renderer>().sharedMaterial = mat;
    }
    RenderSettings.ambientLight = new Color(.2f, .2f, .2f);
    EditorSceneManager.SaveScene(go.scene);
    var reference = JsonUtility.FromJson<Reference>(
        File.ReadAllText("Assets/Rigs/" + Name + ".json"));
    ExchangeBuild.Require(Mathf.Abs(clip.length - reference.duration) < .0001f,
                          "Rig sample duration");
    var graph = PlayableGraph.Create("Rig import validation");
    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    var playable = AnimationClipPlayable.Create(graph, clip);
    playable.SetApplyFootIK(false);
    AnimationPlayableOutput.Create(graph, "Rigs", preview.animator)
        .SetSourcePlayable(playable);
    graph.Play();
    var result = new Report { duration = clip.length };
    Reference surface = Name.Contains("contacto")
                            ? JsonUtility.FromJson<Reference>(File.ReadAllText(
                                  "Assets/Rigs/" + Name + "_surface.json"))
                            : null;
    var cam = GameObject.Find("RigReviewCamera").GetComponent<Camera>();
    var anchor = go.transform.parent;
    var baked = new Mesh();
    var sheep = go.GetComponentsInChildren<SkinnedMeshRenderer>().Single(
        s => s.name == "Sheep_Quaternius");
    ExchangeBuild.Require(sheep.sharedMesh.blendShapeCount == 1,
                          "Neck compression blend shape");
    foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
      result.triangles += renderer.sharedMesh.triangles.Length / 3;
    ExchangeBuild.Require(result.triangles ==
                              (Name.StartsWith("10") ? 612 : 11162),
                          "Triangle preservation " + result.triangles);
    foreach (var s in reference.samples) {
      playable.SetTime(s.time);
      graph.Evaluate(0);
      ExchangeBuild.Require(anchor.localPosition == Vector3.zero &&
                                anchor.localRotation == Quaternion.identity &&
                                anchor.localScale == Vector3.one,
                            "AR anchor moved");
      foreach (var m in s.meshes) {
        var skin = go.GetComponentsInChildren<SkinnedMeshRenderer>().Single(
            r => r.name == m.name);
        skin.BakeMesh(baked, true);
        var points =
            baked.vertices.Select(v => skin.transform.TransformPoint(v))
                .ToArray();
        foreach (var expected in m.vertices) {
          result.maxVertexError =
              Mathf.Max(result.maxVertexError,
                        points.Min(p => Vector3.Distance(
                                       p, ExchangeBuild.Convert(expected))));
          result.verticesCompared++;
        }
        result.minHeight = Mathf.Min(result.minHeight, points.Min(p => p.y));
      }
      result.maxShapeError =
          Mathf.Max(result.maxShapeError,
                    Mathf.Abs(sheep.GetBlendShapeWeight(0) / 100 - s.shape));
      if (Name.Contains("contacto")) {
        sheep.BakeMesh(baked, true);
        var woolPoints =
            baked.vertices.Select(v => sheep.transform.TransformPoint(v))
                .ToArray();
        var woolTriangles = baked.triangles;
        var toothSample = surface.samples[result.samples];
        foreach (var pair in new[] { ("Chupa_Detail_Ivory", toothSample.upper),
                                     ("Chupa_LowerTeeth",
                                      toothSample.lower) }) {
          var tooth = go.GetComponentsInChildren<SkinnedMeshRenderer>().Single(
              r => r.name == pair.Item1);
          tooth.BakeMesh(baked, true);
          var expected = ExchangeBuild.Convert(pair.Item2);
          var actual =
              baked.vertices.Select(v => tooth.transform.TransformPoint(v))
                  .OrderBy(v => (v - expected).sqrMagnitude)
                  .First();
          result.maxVertexError = Mathf.Max(result.maxVertexError,
                                            Vector3.Distance(actual, expected));
          result.maxSurfaceContact =
              Mathf.Max(result.maxSurfaceContact,
                        SurfaceDistance(actual, woolPoints, woolTriangles));
        }
        foreach (var pair in new[] { ("UpperContact", "NeckUpper"),
                                     ("LowerContact", "NeckLower") }) {
          var a = ExchangeBuild.Find(go, pair.Item1).position;
          var b = ExchangeBuild.Find(go, pair.Item2).position;
          result.maxContact =
              Mathf.Max(result.maxContact, Vector3.Distance(a, b));
        }
      }
      if (!string.IsNullOrEmpty(s.label)) {
        ExchangeBuild.SaveCamera(cam, Evidence + "/" + s.label + ".png");
        if (Name.Contains("contacto") && s.time == 0) {
          var pos = cam.transform.position;
          var rot = cam.transform.rotation;
          cam.transform.position = new Vector3(2.5f, 1.4f, 3);
          cam.transform.LookAt(new Vector3(0, .4f, 1));
          ExchangeBuild.SaveCamera(cam, Evidence + "/contact_close.png");
          cam.transform.SetPositionAndRotation(pos, rot);
        }
      }
      result.samples++;
    }
    anchor.position = new Vector3(2, 3, 4);
    anchor.rotation = Quaternion.Euler(10, 35, 5);
    anchor.localScale = Vector3.one * .1f;
    var expectedAnchor = anchor.localToWorldMatrix;
    playable.SetTime(reference.duration / 2);
    graph.Evaluate(0);
    ExchangeBuild.Require(anchor.localToWorldMatrix == expectedAnchor,
                          "External AR pose overwritten");
    graph.Destroy();
    File.WriteAllText(Evidence + "/checks.json",
                      JsonUtility.ToJson(result, true) + "\n");
    ExchangeBuild.Require(result.maxVertexError < .001f,
                          "Skin error " + result.maxVertexError);
    ExchangeBuild.Require(result.maxShapeError < .001f,
                          "Shape error " + result.maxShapeError);
    ExchangeBuild.Require(result.minHeight > -.001f,
                          "Ground penetration " + result.minHeight);
    ExchangeBuild.Require(result.maxContact < .001f,
                          "Contact error " + result.maxContact);
    ExchangeBuild.Require(result.maxSurfaceContact < .001f,
                          "Surface contact error " + result.maxSurfaceContact);
    result.passed = true;
    File.WriteAllText(Evidence + "/checks.json",
                      JsonUtility.ToJson(result, true) + "\n");
    TrackingChecks.Run();
    Debug.Log("CHUPACABRAS_RIG_OK " + JsonUtility.ToJson(result));
  }
}
