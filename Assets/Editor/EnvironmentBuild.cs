using System;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EnvironmentBuild
{
    public const string Scene = "Assets/Scenes/06_Environment.unity";
    const string Folder = "Assets/Environment";
    public static readonly string Evidence =
        Environment.GetEnvironmentVariable("CHUPA_ENV_EVIDENCE") ?? "docs/evidencias/2026-09-24_escenario";
    [Serializable]
    public class Palette
    {
        public Entry[] entries;
    }
    [Serializable]
    public class Entry
    {
        public string name;
        public Color color;
    }
    public static void Configure()
    {
        TrackingBuild.RequireVersion();
        ExchangeBuild.Require(!File.Exists(Scene), "Preserve environment scene");
        Directory.CreateDirectory(Evidence);
        ExchangeBuild.ConfigureModel(Folder + "/06_escenario_contexto.fbx");
        var importer = (ModelImporter)AssetImporter.GetAtPath(Folder + "/06_escenario.fbx");
        importer.globalScale = 1;
        importer.useFileScale = true;
        importer.bakeAxisConversion = true;
        importer.importAnimation = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.SaveAndReimport();
        EditorSceneManager.OpenScene(BlockingBuild.Scene);
        var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
        UnityEngine.Object.DestroyImmediate(player.animator.gameObject);
        var context = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/06_escenario_contexto.fbx"));
        player.animator = context.GetComponent<Animator>();
        if (player.animator == null)
            player.animator = context.AddComponent<Animator>();
        player.animator.applyRootMotion = false;
        player.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        player.clip = ExchangeBuild.Clip(Folder + "/06_escenario_contexto.fbx");
        // Reuse existing provisional actor materials; the earlier scene/assets stay
        // untouched.
        foreach (var t in context.GetComponentsInChildren<Transform>())
            t.gameObject.layer = 8;
        foreach (var r in context.GetComponentsInChildren<Renderer>())
        {
            if (r.name.StartsWith("Sheep") || r.name.StartsWith("Creature"))
                r.sharedMaterial =
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Preview/05_r04_" + r.name + ".mat");
            else
                r.gameObject.SetActive(false);
        }
        var cam = ExchangeBuild.Find(context, "CinemaCamera").GetComponent<Camera>();
        cam.cullingMask = 1 << 8;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.035f, .045f, .09f);
        cam.nearClipPlane = .05f;
        cam.farClipPlane = 100;
        cam.aspect = 16f / 9;
        cam.gateFit = Camera.GateFitMode.Horizontal;
        cam.targetTexture =
            AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/Preview/05_r04_CinemaRT.renderTexture");
        cam.depth = -1;
        var env = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/06_escenario.fbx"));
        var palette = JsonUtility.FromJson<Palette>(File.ReadAllText(Folder + "/palette.json"));
        foreach (var t in env.GetComponentsInChildren<Transform>())
            t.gameObject.layer = 8;
        foreach (var r in env.GetComponentsInChildren<Renderer>())
        {
            var entry = palette.entries.Single(e => e.name == r.name);
            var mat = new Material(Shader.Find(r.name == "Env_Moon" ? "Universal Render Pipeline/Unlit"
                                                                    : "Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", entry.color);
            mat.SetFloat("_Smoothness", 0);
            AssetDatabase.CreateAsset(mat, Folder + "/" + r.name + ".mat");
            r.sharedMaterial = mat;
        }
        player.clip.SampleAnimation(context, 0);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(context.scene, Scene);
        Verify();
    }
    public static void Verify()
    {
        TrackingBuild.RequireVersion();
        EditorSceneManager.OpenScene(Scene);
        Directory.CreateDirectory(Evidence);
        var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
        var cam = ExchangeBuild.Find(player.animator.gameObject, "CinemaCamera").GetComponent<Camera>();
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                            .Where(r => r.gameObject.layer == 8)
                            .ToArray();
        var environment = renderers.Where(r => r.name.StartsWith("Env_")).ToArray();
        int triangles = environment.Sum(r => r.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3);
        ExchangeBuild.Require(environment.Length == 10 && triangles < 12000, "Static geometry budget");
        ExchangeBuild.Require(
            environment.All(r => r.sharedMaterial != null && r.sharedMaterial.shader.isSupported),
            "Materials");
        ExchangeBuild.Require(Mathf.Abs(player.clip.length - 40) < .0001f, "40 seconds");
        // Actual rendered depth checks at every critical frame, including the new
        // set.
        var saved = renderers.Select(r => r.sharedMaterials).ToArray();
        var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        black.SetColor("_BaseColor", Color.black);
        var white = new Material(black);
        white.SetColor("_BaseColor", Color.white);
        var previous = cam.targetTexture;
        var bg = cam.backgroundColor;
        var rt = new RenderTexture(320, 180, 24);
        cam.targetTexture = rt;
        cam.backgroundColor = Color.black;
        var image = new Texture2D(320, 180, TextureFormat.RGB24, false);
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
        Mask(r => r.name.StartsWith("CreatureEye"));
        int eyes = Count(10);
        ExchangeBuild.Require(eyes > 0, "Eyes in stalking shot");
        Mask(r => r.name.StartsWith("Creature"));
        for (int f = 450; f < 600; f++)
            ExchangeBuild.Require(Count(f / 30.0) == 0, "Stalking hidden at " + f);
        Mask(r => r.name.StartsWith("Sheep"));
        int before = Count(10), after = Count(25.5);
        ExchangeBuild.Require(before > 20 && after > 20, "Sheep readable");
        for (int f = 636; f < 750; f++)
            ExchangeBuild.Require(Count(f / 30.0) == 0, "Sheep hidden at " + f);
        Mask(r => r.name.StartsWith("Sheep") || r.name.StartsWith("Creature"));
        for (int f = 1170; f < 1200; f++)
            ExchangeBuild.Require(Count(f / 30.0) == 0, "Empty end at " + f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterials = saved[i];
        cam.targetTexture = previous;
        cam.backgroundColor = bg;
        rt.Release();
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(image);
        foreach (float t in new[] { 0, 10, 15, 20.3f, 21.2f, 25.5f, 28, 31, 34, 37, 39 })
        {
            player.Evaluate(t);
            ExchangeBuild.SaveCamera(
                cam, Evidence + "/frame_" +
                         t.ToString("00.0", System.Globalization.CultureInfo.InvariantCulture) + ".png");
        }
        player.Evaluate(10);
        cam.Render();
        ExchangeBuild.SaveCamera(GameObject.Find("ExteriorPreviewCamera").GetComponent<Camera>(),
                                 Evidence + "/window.png");
        File.WriteAllText(Evidence + "/checks.json",
                          "{\n  \"passed\":true, \"physicalDevice\":false, \"triangles\":" + triangles +
                              (", \"staticRenderers\":10, \"sharedMaterials\":10, " +
                               "\"hiddenCreatureFrames\":150, \"hiddenSheepFrames\":114, " +
                               "\"emptyFrames\":30, \"eyesPixels\":") +
                              eyes + ", \"sheepPixelsBefore\":" + before +
                              ", \"sheepPixelsAfter\":" + after + "\n}\n");
        TrackingChecks.Run();
        Debug.Log("CHUPACABRAS_ENVIRONMENT_OK");
    }
}
