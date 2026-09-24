using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class ExchangeBuild
{
    public const string Model = "Assets/Exchange/04_intercambio.fbx";
    public static readonly string Evidence = Environment.GetEnvironmentVariable("CHUPA_EXCHANGE_EVIDENCE") ??
                                             "docs/evidencias/2026-09-24_intercambio";
    [Serializable]
    public class Sample
    {
        public float time, shape;
        public Vector3 mouth, neck;
        public Vector3[] vertices;
    }
    [Serializable]
    public class Reference
    {
        public float duration;
        public Sample[] samples;
    }
    public static Transform Find(GameObject root, string name) =>
        root.GetComponentsInChildren<Transform>(true).Single(t => t.name == name);
    public static AnimationClip Clip(string path) => AssetDatabase.LoadAllAssetsAtPath(path)
                                                         .OfType<AnimationClip>()
                                                         .Single(c => !c.name.StartsWith("__preview__"));
    public static void ConfigureModel(string path)
    {
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.globalScale = 1;
        importer.useFileScale = true;
        importer.bakeAxisConversion = true;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.importAnimation = true;
        importer.importCameras = true;
        importer.importBlendShapes = true;
        importer.isReadable = true;
        importer.animationCompression = ModelImporterAnimationCompression.Off;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        var clips = importer.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.loopTime = false;
            c.loopPose = false;
            c.keepOriginalPositionXZ = true;
            c.keepOriginalPositionY = true;
            c.keepOriginalOrientation = true;
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }
    public static Vector3 Convert(Vector3 v) => new Vector3(v.x, v.z, v.y);
    public static void Require(bool ok, string message)
    {
        if (!ok)
            throw new Exception(message);
    }
    public static void Verify()
    {
        TrackingBuild.RequireVersion();
        Directory.CreateDirectory(Evidence);
        ConfigureModel(Model);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go =
            (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
        var clip = Clip(Model);
        Require(Mathf.Abs(clip.length - 40) < .0001f, "Duration is not 40 seconds");
        Require(Vector3.Distance(Find(go, "AxisX").position, Vector3.right) < .0001f, "X axis");
        Require(Vector3.Distance(Find(go, "AxisY").position, Vector3.forward) < .0001f, "Y axis");
        Require(Vector3.Distance(Find(go, "AxisZ").position, Vector3.up) < .0001f, "Z axis");
        var meter = Find(go, "MeterCube").GetComponent<Renderer>();
        Require(Vector3.Distance(meter.bounds.size, Vector3.one) < .0001f, "Meters");
        var skin = go.GetComponentInChildren<SkinnedMeshRenderer>();
        Require(skin.sharedMesh.blendShapeCount == 1, "Missing wool blendshape");
        var reference =
            JsonUtility.FromJson<Reference>(File.ReadAllText("Assets/Exchange/04_intercambio.json"));
        float maxContact = 0, maxVertex = 0, maxPoint = 0, maxShape = 0;
        var mesh = new Mesh();
        foreach (var sample in reference.samples)
        {
            clip.SampleAnimation(go, sample.time);
            var mouth = Find(go, "Mouth").position;
            var neck = Find(go, "Neck").position;
            maxContact = Mathf.Max(maxContact, Vector3.Distance(mouth, neck));
            maxPoint = Mathf.Max(maxPoint, Vector3.Distance(mouth, Convert(sample.mouth)));
            maxShape = Mathf.Max(maxShape, Mathf.Abs(skin.GetBlendShapeWeight(0) / 100 - sample.shape));
            skin.BakeMesh(mesh);
            foreach (var expected in sample.vertices)
                maxVertex = Mathf.Max(
                    maxVertex, mesh.vertices.Min(v => Vector3.Distance(skin.transform.TransformPoint(v),
                                                                       Convert(expected))));
        }
        Require(maxContact < .001f && maxPoint < .001f, "Contact mismatch " + maxContact + "/" + maxPoint);
        Require(maxVertex < .001f && maxShape < .001f, "Deformation mismatch " + maxVertex + "/" + maxShape);
        // Face normals point outward on the reference cube, after handedness conversion.
        var cubeMesh = meter.GetComponent<MeshFilter>().sharedMesh;
        for (int i = 0; i < cubeMesh.vertexCount; i++)
            Require(Vector3.Dot(cubeMesh.vertices[i], cubeMesh.normals[i]) > 0, "Reversed normal");
        File.WriteAllText(
            Evidence + "/checks_" + DateTime.UtcNow.ToString("HHmmss") + ".json",
            JsonUtility.ToJson(new Result { samples = reference.samples.Length, maxContact = maxContact,
                                            maxPoint = maxPoint, maxVertex = maxVertex, maxShape = maxShape },
                               true) +
                "\n");
        if (!File.Exists("Assets/Scenes/04_Exchange.unity"))
            CreateScene(go, clip, "Assets/Scenes/04_Exchange.unity", "04");
        TrackingChecks.Run();
        Debug.Log("CHUPACABRAS_EXCHANGE_OK");
    }
    [Serializable]
    public class Result
    {
        public bool passed = true, physicalDevice = false;
        public int samples;
        public float duration = 40, maxContact, maxPoint, maxVertex, maxShape;
    }
    public static void CreateScene(GameObject go, AnimationClip clip, string scenePath, string prefix)
    {
        if (File.Exists(scenePath))
            throw new IOException("Preserve scene: " + scenePath);
        Directory.CreateDirectory("Assets/Preview");
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = 8;
        foreach (var a in go.GetComponentsInChildren<Animator>())
            UnityEngine.Object.DestroyImmediate(a);
        go.AddComponent<Animator>();
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            Color color = new Color(.22f, .6f, .56f);
            // Blender's solid blockout palette is reproduced explicitly in URP.
            if (renderer.name.StartsWith("Sheep"))
                color = new Color(.78f, .74f, .58f);
            if (renderer.name.StartsWith("Creature"))
                color = new Color(.06f, .085f, .08f);
            if (renderer.name.StartsWith("SheepHead") || renderer.name.StartsWith("SheepLeg"))
                color = new Color(.045f, .04f, .035f);
            if (renderer.name == "BarnDoor")
                color = new Color(.06f, .04f, .03f);
            if (renderer.name.Contains("Eye"))
                color = new Color(1, .7f, .12f);
            if (renderer.name.StartsWith("Ground"))
                color = new Color(.18f, .15f, .14f);
            if (renderer.name.StartsWith("Barn"))
                color = new Color(.28f, .18f, .15f);
            if (renderer.name.StartsWith("Moon"))
                color = new Color(.78f, .79f, .67f);
            string materialPath = "Assets/Preview/" + prefix + "_" + renderer.name + ".mat";
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0);
            AssetDatabase.CreateAsset(mat, materialPath);
            renderer.sharedMaterial = mat;
        }
        var cinema = Find(go, "CinemaCamera").GetComponent<Camera>();
        foreach (var cam in go.GetComponentsInChildren<Camera>())
            cam.enabled = cam == cinema;
        cinema.cullingMask = 1 << 8;
        cinema.clearFlags = CameraClearFlags.SolidColor;
        cinema.backgroundColor = new Color(.035f, .045f, .09f);
        cinema.nearClipPlane = .05f;
        cinema.farClipPlane = 100;
        cinema.aspect = 16f / 9;
        cinema.gateFit = Camera.GateFitMode.Horizontal;
        cinema.GetUniversalAdditionalCameraData();
        var rt = new RenderTexture(960, 540, 24) { name = prefix + "_CinemaRT" };
        AssetDatabase.CreateAsset(rt, "Assets/Preview/" + prefix + "_CinemaRT.renderTexture");
        cinema.targetTexture = rt;
        cinema.depth = -1;
        var light = new GameObject("CinemaLight").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 2;
        light.cullingMask = 1 << 8;
        light.transform.rotation = Quaternion.Euler(45, -30, 0);
        light.shadows = LightShadows.None;
        var anchor = new GameObject("MarkerReference_100mm").transform;
        var tag = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tag.name = "MarkerReference";
        tag.transform.SetParent(anchor, false);
        tag.transform.localScale = new Vector3(.32f, .32f, 1);
        var tagMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        tagMat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/AprilTagFixture.png");
        // Fixture detection square is 160/512 pixels: 320 mm image gives 100 mm between corners.
        AssetDatabase.CreateAsset(tagMat, "Assets/Preview/" + prefix + "_Marker.mat");
        tag.GetComponent<Renderer>().sharedMaterial = tagMat;
        var window = GameObject.CreatePrimitive(PrimitiveType.Quad);
        window.name = "Window_240x135mm";
        window.transform.SetParent(anchor, false);
        window.transform.localPosition = new Vector3(.30f, 0, -.001f);
        window.transform.localScale = new Vector3(.24f, .135f, 1);
        var windowMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        windowMat.mainTexture = rt;
        AssetDatabase.CreateAsset(windowMat, "Assets/Preview/" + prefix + "_Window.mat");
        window.GetComponent<Renderer>().sharedMaterial = windowMat;
        var view = new GameObject("ExteriorPreviewCamera").AddComponent<Camera>();
        view.transform.position = new Vector3(.17f, 0, -.8f);
        view.fieldOfView = 35;
        view.nearClipPlane = .01f;
        view.cullingMask = 1;
        view.clearFlags = CameraClearFlags.SolidColor;
        view.backgroundColor = new Color(.12f, .13f, .15f);
        view.GetUniversalAdditionalCameraData();
        var player = new GameObject("Preliminary40SecondPlayer").AddComponent<Chupacabras.SequencePreview>();
        player.animator = go.GetComponent<Animator>();
        player.clip = clip;
        player.animator.applyRootMotion = false;
        player.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        clip.SampleAnimation(go, 0);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(go.scene, scenePath);
    }
    public static void Capture()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/04_Exchange.unity");
        var player = UnityEngine.Object.FindFirstObjectByType<Chupacabras.SequencePreview>();
        var cinema = Find(player.animator.gameObject, "CinemaCamera").GetComponent<Camera>();
        foreach (float time in new[] { 0f, 10f, 20f, 30f, 40f })
        {
            player.clip.SampleAnimation(player.animator.gameObject, time);
            SaveCamera(cinema, Evidence + "/cinema_" + time + ".png");
        }
        SaveCamera(GameObject.Find("ExteriorPreviewCamera").GetComponent<Camera>(), Evidence + ("/window." +
                                                                                                "png"));
    }
    public static void SaveCamera(Camera cam, string path)
    {
        if (File.Exists(path))
            throw new IOException("Preserve capture: " + path);
        var previous = cam.targetTexture;
        var rt = previous != null ? previous : new RenderTexture(960, 540, 24);
        cam.targetTexture = rt;
        rt.Create();
        cam.Render();
        var active = RenderTexture.active;
        RenderTexture.active = rt;
        var image = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        image.Apply();
        File.WriteAllBytes(path, image.EncodeToPNG());
        RenderTexture.active = active;
        cam.targetTexture = previous;
        UnityEngine.Object.DestroyImmediate(image);
        if (previous == null)
        {
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }
    public static void VerifyRuntimeAndWindow()
    {
        TrackingBuild.RequireVersion();
        Directory.CreateDirectory(Evidence);
        EditorSceneManager.OpenScene("Assets/Scenes/04_Exchange.unity");
        var player = UnityEngine.Object.FindFirstObjectByType<Chupacabras.SequencePreview>();
        var root = player.animator.gameObject;
        var reference =
            JsonUtility.FromJson<Reference>(File.ReadAllText("Assets/Exchange/04_intercambio.json"));
        float error = 0;
        foreach (var sample in reference.samples)
        {
            if (sample.time == 40)
                continue;
            player.Evaluate(sample.time);
            error = Mathf.Max(error, Vector3.Distance(Find(root, "Mouth").position, Convert(sample.mouth)));
        }
        Require(error < .001f, "Runtime differs from Blender " + error);
        player.Evaluate(80);
        Require(player.seconds == 0, "40-second reset");
        var window = GameObject.Find("Window_240x135mm");
        var mat = window.GetComponent<Renderer>().sharedMaterial;
        var original = mat.mainTexture;
        var pattern = new Texture2D(4, 4, TextureFormat.RGB24, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                pattern.SetPixel(
                    x, y, y >= 2 ? (x < 2 ? Color.red : Color.green) : (x < 2 ? Color.blue : Color.yellow));
        pattern.Apply();
        mat.mainTexture = pattern;
        var exterior = GameObject.Find("ExteriorPreviewCamera").GetComponent<Camera>();
        string path = Evidence + "/window_orientation.png";
        SaveCamera(exterior, path);
        var image = new Texture2D(2, 2);
        image.LoadImage(File.ReadAllBytes(path));
        Vector3[] points = { new Vector3(-.25f, .25f, 0), new Vector3(.25f, .25f, 0),
                             new Vector3(-.25f, -.25f, 0), new Vector3(.25f, -.25f, 0) };
        Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };
        exterior.aspect = 16f / 9;
        for (int i = 0; i < points.Length; i++)
        {
            var screen = exterior.WorldToViewportPoint(window.transform.TransformPoint(points[i]));
            var pixel = image.GetPixel((int)(screen.x * image.width), (int)(screen.y * image.height));
            Require(Vector3.Distance(new Vector3(pixel.r, pixel.g, pixel.b),
                                     new Vector3(colors[i].r, colors[i].g, colors[i].b)) < .08f,
                    "Window UV orientation " + i + " " + pixel);
        }
        var a = exterior.WorldToViewportPoint(window.transform.TransformPoint(new Vector3(-.5f, -.5f, 0)));
        var b = exterior.WorldToViewportPoint(window.transform.TransformPoint(new Vector3(.5f, .5f, 0)));
        float aspect = (b.x - a.x) * image.width / ((b.y - a.y) * image.height);
        Require(Mathf.Abs(aspect - 16f / 9) < .001f, "Window aspect " + aspect);
        mat.mainTexture = original;
        UnityEngine.Object.DestroyImmediate(pattern);
        UnityEngine.Object.DestroyImmediate(image);
        File.WriteAllText(
            Evidence + "/runtime_window.json",
            "{\"passed\":true,\"runtime_contact_error_m\":" +
                error.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                ",\"four_uv_corners\":true,\"aspect_16_9\":true,\"physical_device\":false}\n");
        Debug.Log("CHUPACABRAS_RUNTIME_WINDOW_OK");
    }
    public static void Inspect()
    {
        TrackingBuild.RequireVersion();
        ConfigureModel(Model);
        var go =
            (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
        var clip = Clip(Model);
        clip.SampleAnimation(go, 0);
        var lines = go.GetComponentsInChildren<Transform>()
                        .Select(t => t.name + " p=" + t.position.ToString("F4") + " s=" +
                                     t.lossyScale.ToString("F4") + " r=" + t.eulerAngles.ToString("F2"))
                        .ToList();
        lines.Add("clip " + clip.length + " frames " + clip.frameRate);
        foreach (var s in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            lines.Add(s.name + " shapes=" + s.sharedMesh.blendShapeCount + " bounds=" + s.bounds);
        File.WriteAllLines(Evidence + "/import_inspect.txt", lines);
    }
}
