using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Chupacabras;

public static class BlockingBuild
{
    public const string Model = "Assets/Exchange/05_blocking_r04.fbx";
    public const string Scene = "Assets/Scenes/05_Blocking_r04.unity";
    public static readonly string Evidence = Environment.GetEnvironmentVariable("CHUPA_BLOCKING_EVIDENCE") ??
                                             "docs/evidencias/2026-09-24_blocking_r04";
    [Serializable]
    public class Reference
    {
        public float duration, landing;
        public Sample[] samples;
    }
    [Serializable]
    public class Sample
    {
        public float time;
        public Vector3 camera, pair;
    }
    [Serializable]
    public class Result
    {
        public bool passed, physicalDevice = false;
        public int hiddenCreatureFrames, hiddenSheepFrames, emptyFrames;
        public float maxContact, maxCamera, maxPair, clipDuration;
        public int sheepPixelsBefore, sheepPixelsAfter, creaturePixelsCalm, eyePixelsCalm;
        public string clock = "preliminary Playables; final Timeline remains step 17";
    }
    public static void Configure()
    {
        TrackingBuild.RequireVersion();
        Directory.CreateDirectory(Evidence);
        ExchangeBuild.ConfigureModel(Model);
        if (File.Exists(Scene))
            throw new IOException("Preserve existing scene: " + Scene);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root =
            (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
        ExchangeBuild.CreateScene(root, ExchangeBuild.Clip(Model), Scene, "05_r04");
    }
    public static void VerifyAndCapture()
    {
        TrackingBuild.RequireVersion();
        EditorSceneManager.OpenScene(Scene);
        Directory.CreateDirectory(Evidence);
        var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
        var root = player.animator.gameObject;
        var cam = ExchangeBuild.Find(root, "CinemaCamera").GetComponent<Camera>();
        var mouth = ExchangeBuild.Find(root, "Mouth");
        var neck = ExchangeBuild.Find(root, "Neck");
        var result = new Result { clipDuration = player.clip.length };
        ExchangeBuild.Require(Mathf.Abs(result.clipDuration - 40) < .0001f, "Clip duration");
        var reference =
            JsonUtility.FromJson<Reference>(File.ReadAllText("Assets/Exchange/05_blocking_r04.json"));
        foreach (var sample in reference.samples)
        {
            // Same playable graph as actual playback; no Editor SampleAnimation shortcut.
            player.Evaluate(sample.time);
            if (sample.time == 40)
                continue; // boundary intentionally resets to zero
            result.maxCamera =
                Mathf.Max(result.maxCamera,
                          Vector3.Distance(cam.transform.position, ExchangeBuild.Convert(sample.camera)));
            result.maxPair =
                Mathf.Max(result.maxPair, Vector3.Distance(ExchangeBuild.Find(root, "PairPath").position,
                                                           ExchangeBuild.Convert(sample.pair)));
        }
        for (int f = 636; f < 1200; f++)
        {
            player.Evaluate(f / 30.0);
            result.maxContact = Mathf.Max(result.maxContact, Vector3.Distance(mouth.position, neck.position));
        }
        ExchangeBuild.Require(result.maxCamera < .001f && result.maxPair < .001f && result.maxContact < .001f,
                              "Runtime imported curves/contact " + JsonUtility.ToJson(result));
        player.Evaluate(0);
        var initial = ExchangeBuild.Find(root, "PairPath").position;
        player.Evaluate(40);
        ExchangeBuild.Require(Vector3.Distance(initial, ExchangeBuild.Find(root, "PairPath").position) <
                                  .0001f,
                              "Reset boundary");
        // RGB masks use the actual scene depth, with no dust, hiding flags or camera tricks.
        var renderers = root.GetComponentsInChildren<Renderer>();
        var saved = renderers.Select(r => r.sharedMaterial).ToArray();
        var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        black.SetColor("_BaseColor", Color.black);
        var white = new Material(black);
        white.SetColor("_BaseColor", Color.white);
        var oldRT = cam.targetTexture;
        var oldBackground = cam.backgroundColor;
        var maskRT = new RenderTexture(320, 180, 24);
        cam.targetTexture = maskRT;
        cam.backgroundColor = Color.black;
        var image = new Texture2D(320, 180, TextureFormat.RGB24, false);
        void Mask(string prefix)
        {
            foreach (var r in renderers)
                r.sharedMaterial = r.name.StartsWith(prefix) ? white : black;
        }
        int Count(double t)
        {
            player.Evaluate(t);
            cam.Render();
            var active = RenderTexture.active;
            RenderTexture.active = maskRT;
            image.ReadPixels(new Rect(0, 0, 320, 180), 0, 0);
            image.Apply();
            RenderTexture.active = active;
            return image.GetPixels32().Count(p => p.r > 200 && p.g > 200 && p.b > 200);
        }
        Mask("Sheep");
        result.sheepPixelsBefore = Count(10);
        result.sheepPixelsAfter = Count(25.5);
        ExchangeBuild.Require(result.sheepPixelsBefore > 20 && result.sheepPixelsAfter > 20,
                              "Sheep visible before/after attack");
        for (int f = 636; f < 750; f++)
        {
            int pixels = Count(f / 30.0);
            if (pixels != 0)
            {
                File.WriteAllBytes(Evidence + "/failed_sheep_" + f + ".png", image.EncodeToPNG());
                throw new Exception("Sheep visible at " + f / 30.0 + ": " + pixels);
            }
            result.hiddenSheepFrames++;
        }
        Mask("CreatureEye");
        result.eyePixelsCalm = Count(10);
        ExchangeBuild.Require(result.eyePixelsCalm > 0, "Eyes absent during stalking");
        Mask("Creature");
        result.creaturePixelsCalm = Count(10);
        for (int f = 450; f < 600; f++)
        {
            int pixels = Count(f / 30.0);
            if (pixels != 0)
            {
                File.WriteAllBytes(Evidence + "/failed_creature_" + f + ".png", image.EncodeToPNG());
                throw new Exception("Creature visible at " + f / 30.0 + ": " + pixels);
            }
            result.hiddenCreatureFrames++;
        }
        foreach (var r in renderers)
            r.sharedMaterial = (r.name.StartsWith("Sheep") || r.name.StartsWith("Creature")) ? white : black;
        for (int f = 1170; f < 1200; f++)
        {
            ExchangeBuild.Require(Count(f / 30.0) == 0, "Not empty at " + f / 30.0);
            result.emptyFrames++;
        }
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterial = saved[i];
        cam.targetTexture = oldRT;
        cam.backgroundColor = oldBackground;
        maskRT.Release();
        UnityEngine.Object.DestroyImmediate(maskRT);
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(black);
        UnityEngine.Object.DestroyImmediate(white);
        var exterior = GameObject.Find("ExteriorPreviewCamera").GetComponent<Camera>();
        ExchangeBuild.Require(exterior.cullingMask == 1 && cam.cullingMask == (1 << 8), "Cinema isolation");
        player.Evaluate(23);
        ExchangeBuild.SaveCamera(cam, Evidence + "/internal_23.png");
        var cinemaPosition = cam.transform.position;
        var cinemaRotation = cam.transform.rotation;
        for (int i = 0; i < 3; i++)
        {
            exterior.transform.position = new Vector3(i == 0   ? .17f
                                                      : i == 1 ? -.06f
                                                               : .4f,
                                                      .08f * (i - 1), -.8f);
            exterior.transform.LookAt(new Vector3(.17f, 0, 0));
            ExchangeBuild.SaveCamera(exterior, Evidence + "/exterior_" + i + ".png");
            ExchangeBuild.Require(cam.transform.position == cinemaPosition &&
                                      cam.transform.rotation == cinemaRotation,
                                  "Exterior changed cinema");
        }
        ExchangeBuild.SaveCamera(cam, Evidence + "/internal_23_after.png");
        ExchangeBuild.Require(File.ReadAllBytes(Evidence + "/internal_23.png")
                                  .SequenceEqual(File.ReadAllBytes(Evidence + "/internal_23_after.png")),
                              "Exterior leaked into cinema");
        result.passed = true;
        File.WriteAllText(Evidence + "/checks.json", JsonUtility.ToJson(result, true) + "\n");
        foreach (float t in new[] { 0, 10, 15, 20, 20.3f, 20.8f, 21.2f, 23, 25, 28, 31, 34, 37, 39 })
        {
            player.Evaluate(t);
            ExchangeBuild.SaveCamera(
                cam, Evidence + "/frame_" +
                         t.ToString("00.0", System.Globalization.CultureInfo.InvariantCulture) + ".png");
        }
        Debug.Log("CHUPACABRAS_BLOCKING_OK " + JsonUtility.ToJson(result));
    }
    public static void BuildAndroid()
    {
        TrackingBuild.RequireVersion();
        string output = "builds/android/05_blocking_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".apk";
        if (File.Exists(output))
            throw new IOException("Preserve APK " + output);
        var target = UnityEditor.Build.NamedBuildTarget.Android;
        string oldName = PlayerSettings.productName, oldId = PlayerSettings.GetApplicationIdentifier(target),
               oldVersion = PlayerSettings.bundleVersion;
        int oldCode = PlayerSettings.Android.bundleVersionCode;
        try
        {
            PlayerSettings.productName = "Chupacabras — Bloqueo sin cámara AR";
            PlayerSettings.SetApplicationIdentifier(target, "com.chupacabras.ar.blockingpreview");
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 10;
            var report = BuildPipeline.BuildPlayer(
                new BuildPlayerOptions { scenes = new[] { Scene }, locationPathName = output,
                                         target = BuildTarget.Android, options = BuildOptions.Development });
            File.WriteAllText(
                output + ".build.txt",
                $"Unity: {Application.unityVersion}\nResult: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nDuration: {report.summary.totalTime}\nPhysical device: not tested; offline blocking preview only\n");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("Android preview build failed");
            Debug.Log("CHUPACABRAS_BLOCKING_APK_OK " + output);
        }
        finally
        {
            PlayerSettings.productName = oldName;
            PlayerSettings.SetApplicationIdentifier(target, oldId);
            PlayerSettings.bundleVersion = oldVersion;
            PlayerSettings.Android.bundleVersionCode = oldCode;
            AssetDatabase.SaveAssets();
        }
    }
    public static void CaptureVideo()
    {
        EditorSceneManager.OpenScene(Scene);
        var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
        var cam = ExchangeBuild.Find(player.animator.gameObject, "CinemaCamera").GetComponent<Camera>();
        string folder = Evidence + "/frames";
        if (Directory.Exists(folder))
            throw new IOException("Preserve video frames: " + folder);
        Directory.CreateDirectory(folder);
        for (int f = 0; f < 480; f++)
        {
            player.Evaluate(f / 12.0);
            ExchangeBuild.SaveCamera(cam, folder + "/" + f.ToString("D4") + ".png");
        }
    }
}
