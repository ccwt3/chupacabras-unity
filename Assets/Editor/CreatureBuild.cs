using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreatureBuild
{
    static string Name =>
        Environment.GetEnvironmentVariable("CHUPA_MODEL") ?? "08_chupacabras_forma_r03";
    static string Folder => "Assets/Creature/" + Name;
    static string Model => Folder + "/" + Name + ".fbx";
    static string ContextSuffix =>
        Environment.GetEnvironmentVariable("CHUPA_CONTEXT_SUFFIX") ?? "_contexto_r02";
    static string Context => Folder + "/" + Name + ContextSuffix + ".fbx";
    static string Scene => "Assets/Scenes/" + Name + ContextSuffix + ".unity";
    static string Evidence => Environment.GetEnvironmentVariable("CHUPA_CREATURE_EVIDENCE") ??
                              "docs/evidencias/2026-09-24_" + Name + ContextSuffix;
    [Serializable]
    public class Entry
    {
        public string name;
        public int triangles, uv;
        public Vector3 min, max;
        public string[] materials;
    }
    [Serializable]
    public class Palette
    {
        public string name;
        public Color color;
    }
    [Serializable]
    public class Reference
    {
        public int triangles;
        public Entry[] objects;
        public Palette[] palette;
    }
    [Serializable]
    public class Report
    {
        public bool passed, physicalDevice = false, finalRig = false;
        public int triangles, meshes, materials, hiddenSheepFrames, hiddenCreatureFrames,
            emptyFrames;
        public int sheepBefore, sheepAfter, eyesPixels;
        public float maxBoundsError, maxLandmarkError;
        public string[] failures;
    }
    static Reference ReadReference() =>
        JsonUtility.FromJson<Reference>(File.ReadAllText(Folder + "/" + Name + ".json"));
    static void Materials(GameObject go, Reference reference)
    {
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = 8;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r.name == "Sheep_Quaternius_Volume")
                r.sharedMaterials =
                    new[] { AssetDatabase.LoadAssetAtPath<Material>("Assets/Sheep/Wool.mat"),
                            AssetDatabase.LoadAssetAtPath<Material>("Assets/Sheep/Dark.mat") };
            else
            {
                var entry = reference.objects.SingleOrDefault(o => o.name == r.name);
                if (entry == null)
                {
                    r.gameObject.SetActive(false);
                    continue;
                }
                r.sharedMaterials = entry.materials
                                        .Select(n => AssetDatabase.LoadAssetAtPath<Material>(
                                                    Folder + "/" + n + ".mat"))
                                        .ToArray();
            }
        }
    }
    public static void Configure()
    {
        TrackingBuild.RequireVersion();
        ExchangeBuild.Require(!File.Exists(Scene), "Preserve previous creature scene");
        Directory.CreateDirectory(Evidence);
        ExchangeBuild.ConfigureModel(Model);
        var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
        importer.importAnimation = false;
        importer.SaveAndReimport();
        ExchangeBuild.ConfigureModel(Context);
        var reference = ReadReference();
        foreach (var p in reference.palette)
        {
            var mat =
                new Material(Shader.Find(p.name == "Chupa_Eye" ? "Universal Render Pipeline/Unlit"
                                                               : "Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", p.color);
            mat.SetFloat("_Smoothness", .08f);
            if (!File.Exists(Folder + "/" + p.name + ".mat"))
                AssetDatabase.CreateAsset(mat, Folder + "/" + p.name + ".mat");
        }
        var asset = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Model));
        Materials(asset, reference);
        if (!File.Exists(Folder + "/Chupacabras.prefab"))
            PrefabUtility.SaveAsPrefabAsset(asset, Folder + "/Chupacabras.prefab");
        UnityEngine.Object.DestroyImmediate(asset);
        EditorSceneManager.OpenScene(EnvironmentBuild.Scene);
        var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
        UnityEngine.Object.DestroyImmediate(player.animator.gameObject);
        var context = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Context));
        Materials(context, reference);
        player.animator = context.GetComponent<Animator>();
        if (player.animator == null)
            player.animator = context.AddComponent<Animator>();
        player.animator.applyRootMotion = false;
        player.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        player.clip = ExchangeBuild.Clip(Context);
        var cam = ExchangeBuild.Find(context, "CinemaCamera").GetComponent<Camera>();
        cam.cullingMask = 1 << 8;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.035f, .045f, .09f);
        cam.nearClipPlane = .05f;
        cam.farClipPlane = 100;
        cam.aspect = 16f / 9;
        cam.gateFit = Camera.GateFitMode.Horizontal;
        cam.targetTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(
            "Assets/Preview/05_r04_CinemaRT.renderTexture");
        cam.depth = -1;
        player.Evaluate(0);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(context.scene, Scene);
        Verify();
    }
    public static void Verify()
    {
        TrackingBuild.RequireVersion();
        Directory.CreateDirectory(Evidence);
        EditorSceneManager.OpenScene(Scene);
        var reference = ReadReference();
        var result = new Report();
        var failures = new List<string>();
        void Check(bool ok, string error)
        {
            if (!ok && failures.Count < 30)
                failures.Add(error);
        }
        var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
        Check(Mathf.Abs(player.clip.length - 40) < .0001f, "Duration");
        var cam =
            ExchangeBuild.Find(player.animator.gameObject, "CinemaCamera").GetComponent<Camera>();
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                            .Where(r => r.gameObject.layer == 8 && r.gameObject.activeInHierarchy)
                            .ToArray();
        var saved = renderers.Select(r => r.sharedMaterials).ToArray();
        var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        black.SetColor("_BaseColor", Color.black);
        var white = new Material(black);
        white.SetColor("_BaseColor", Color.white);
        var previous = cam.targetTexture;
        var background = cam.backgroundColor;
        var rt = new RenderTexture(320, 180, 24);
        var image = new Texture2D(320, 180, TextureFormat.RGB24, false);
        cam.targetTexture = rt;
        cam.backgroundColor = Color.black;
        void Mask(Func<Renderer, bool> target)
        {
            foreach (var r in renderers)
                r.sharedMaterials = r.sharedMaterials
                                        .Select(
                                            _ => target(r) ? white : black)
                                        .ToArray();
        }
        int Count(double t)
        {
            player.Evaluate(t);
            cam.Render();
            var old = RenderTexture.active;
            RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, 320, 180), 0, 0);
            image.Apply();
            RenderTexture.active = old;
            return image.GetPixels32().Count(p => p.r > 200 && p.g > 200 && p.b > 200);
        }
        Mask(r => r.name.StartsWith("Chupa_Eye"));
        result.eyesPixels = Count(10);
        Check(result.eyesPixels > 0, "Eyes hidden in stalking view");
        Mask(r => r.name.StartsWith("Chupa_"));
        for (int f = 450; f < 600; f++)
        {
            int count = Count(f / 30.0);
            if (count == 0)
                result.hiddenCreatureFrames++;
            else
                Check(false, "Creature visible " + f + ": " + count);
        }
        Mask(r => r.name.StartsWith("Sheep_"));
        result.sheepBefore = Count(10);
        result.sheepAfter = Count(25.5);
        Check(result.sheepBefore > 20 && result.sheepAfter > 20, "Sheep must be readable");
        for (int f = 636; f < 750; f++)
        {
            int count = Count(f / 30.0);
            if (count == 0)
                result.hiddenSheepFrames++;
            else
                Check(false, "Sheep visible " + f + ": " + count);
            if (f == 636 || f == 675 || f == 749)
                ExchangeBuild.SaveCamera(cam, Evidence + "/mask_" + f + ".png");
        }
        Mask(r => r.name.StartsWith("Chupa_") || r.name.StartsWith("Sheep_"));
        for (int f = 1170; f < 1200; f++)
        {
            if (Count(f / 30.0) == 0)
                result.emptyFrames++;
            else
                Check(false, "End not empty " + f);
        }
        for (int f = 636; f < 1200; f++)
        {
            player.Evaluate(f / 30.0);
            result.maxLandmarkError = Mathf.Max(
                result.maxLandmarkError,
                Vector3.Distance(ExchangeBuild.Find(player.animator.gameObject, "Mouth").position,
                                 ExchangeBuild.Find(player.animator.gameObject, "Neck").position));
        }
        Check(result.maxLandmarkError < .001f, "Landmark drift");
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterials = saved[i];
        cam.targetTexture = previous;
        cam.backgroundColor = background;
        rt.Release();
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(image);
        foreach (float t in new[] { 0, 10, 15, 20.3f, 21.2f, 22.5f, 25.5f, 28, 31, 34, 37, 39 })
        {
            player.Evaluate(t);
            ExchangeBuild.SaveCamera(
                cam, Evidence + "/frame_" +
                         t.ToString("00.0", System.Globalization.CultureInfo.InvariantCulture) +
                         ".png");
        }
        player.Evaluate(25.5);
        cam.Render();
        ExchangeBuild.SaveCamera(GameObject.Find("ExteriorPreviewCamera").GetComponent<Camera>(),
                                 Evidence + "/window.png");
        // Separate neutral review; no changes to the saved sequence scene.
        foreach (var r in renderers)
            r.enabled = false;
        var asset = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Chupacabras.prefab"));
        var meshRenderers = asset.GetComponentsInChildren<MeshRenderer>();
        result.meshes = meshRenderers.Length;
        result.materials = meshRenderers.SelectMany(r => r.sharedMaterials).Distinct().Count();
        foreach (var r in meshRenderers)
        {
            var mesh = r.GetComponent<MeshFilter>().sharedMesh;
            result.triangles += mesh.triangles.Length / 3;
            var entry = reference.objects.Single(o => o.name == r.name);
            var points = mesh.vertices.Select(v => r.transform.TransformPoint(v)).ToArray();
            var lo = new Vector3(points.Min(v => v.x), points.Min(v => v.y), points.Min(v => v.z));
            var hi = new Vector3(points.Max(v => v.x), points.Max(v => v.y), points.Max(v => v.z));
            result.maxBoundsError = Mathf.Max(
                result.maxBoundsError, Vector3.Distance(lo, ExchangeBuild.Convert(entry.min)),
                Vector3.Distance(hi, ExchangeBuild.Convert(entry.max)));
            Check(mesh.uv.Length == mesh.vertexCount &&
                      mesh.uv.All(v => float.IsFinite(v.x) && float.IsFinite(v.y)),
                  "UV " + r.name);
            Check(r.sharedMaterials.All(m => m != null && m.shader.isSupported),
                  "Materials " + r.name);
        }
        Check(result.triangles == reference.triangles, "Topology");
        Check(result.maxBoundsError < .001f, "Scale/axes " + result.maxBoundsError);
        cam.orthographic = true;
        cam.orthographicSize = 2.0f;
        var target = new Vector3(0, 1.2f, -.5f);
        foreach (var shot in new[] { ("front", new Vector3(0, 2, 8)),
                                     ("side", new Vector3(8, 2, 0)),
                                     ("back", new Vector3(0, 2, -8)),
                                     ("three_quarter", new Vector3(5, 3.5f, 7)) })
        {
            cam.transform.position = shot.Item2;
            cam.transform.LookAt(target);
            ExchangeBuild.SaveCamera(cam, Evidence + "/model_" + shot.Item1 + ".png");
        }
        cam.orthographicSize = .78f;
        cam.transform.position = new Vector3(2.5f, 1.8f, 4);
        cam.transform.LookAt(new Vector3(0, 1.38f, .9f));
        ExchangeBuild.SaveCamera(cam, Evidence + "/face.png");
        var sheep = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Sheep/Sheep_Quaternius_r04.prefab"));
        sheep.transform.position = new Vector3(2.3f, 0, .15f);
        sheep.transform.rotation = Quaternion.Euler(0, 180, 0);
        cam.orthographicSize = 2.25f;
        cam.transform.position = new Vector3(5, 3.8f, 8);
        cam.transform.LookAt(new Vector3(1, 1.1f, -.4f));
        ExchangeBuild.SaveCamera(cam, Evidence + "/comparison.png");
        result.failures = failures.ToArray();
        result.passed = failures.Count == 0;
        File.WriteAllText(Evidence + "/checks.json", JsonUtility.ToJson(result, true) + "\n");
        TrackingChecks.Run();
        ExchangeBuild.Require(result.passed, "Creature acceptance: " + string.Join("; ", failures));
        Debug.Log("CHUPACABRAS_CREATURE_OK " + JsonUtility.ToJson(result));
    }
}
