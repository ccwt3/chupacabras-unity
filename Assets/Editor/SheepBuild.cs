using System;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class SheepBuild
{
    const string Folder = "Assets/Sheep";
    const string Model = Folder + "/07_oveja_r04.fbx";
    const string PoseModel = Folder + "/07_oveja_r04_poses.fbx";
    const string Scene = "Assets/Scenes/07_Sheep_r04.unity";
    public static readonly string Evidence =
        Environment.GetEnvironmentVariable("CHUPA_SHEEP_EVIDENCE") ?? "docs/evidencias/2026-09-24_oveja";
    [Serializable]
    public class Sample
    {
        public float time;
        public string label;
        public Vector3[] vertices;
    }
    [Serializable]
    public class Reference
    {
        public Sample[] samples;
    }
    [Serializable]
    public class Report
    {
        public bool passed, physicalDevice = false;
        public int triangles, bones, materials, samples;
        public float maxVertexError, standingHeight, minPoseHeight;
        public string source = "Quaternius / Farm Animals / CC0-1.0";
    }
    static void Materials(GameObject root)
    {
        foreach (var r in root.GetComponentsInChildren<Renderer>())
            r.sharedMaterials = new[] { AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Wool.mat"),
                                        AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Dark.mat") };
        foreach (var t in root.GetComponentsInChildren<Transform>())
            t.gameObject.layer = 8;
    }
    public static void Configure()
    {
        TrackingBuild.RequireVersion();
        ExchangeBuild.Require(!File.Exists(Scene), "Preserve sheep scene");
        Directory.CreateDirectory(Evidence);
        ExchangeBuild.ConfigureModel(Model);
        ExchangeBuild.ConfigureModel(PoseModel);
        var im = (ModelImporter)AssetImporter.GetAtPath(Model);
        im.importAnimation = false;
        im.SaveAndReimport();
        foreach (var item in new[] { ("Wool", new Color(.78f, .74f, .61f)),
                                     ("Dark", new Color(.065f, .055f, .045f)) })
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", item.Item2);
            m.SetFloat("_Smoothness", 0);
            if (!File.Exists(Folder + "/" + item.Item1 + ".mat"))
                AssetDatabase.CreateAsset(m, Folder + "/" + item.Item1 + ".mat");
        }
        EditorSceneManager.OpenScene(EnvironmentBuild.Scene);
        var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
        player.clip.SampleAnimation(player.animator.gameObject, 0);
        foreach (var r in player.animator.GetComponentsInChildren<Renderer>(true))
            r.gameObject.SetActive(false);
        UnityEngine.Object.DestroyImmediate(player.gameObject);
        var sheep =
            (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
        Materials(sheep);
        PrefabUtility.SaveAsPrefabAsset(sheep, Folder + "/Sheep_Quaternius_r04.prefab");
        sheep.transform.position = new Vector3(0, 0, .7f);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(sheep.scene, Scene);
        Verify();
    }
    public static void Verify()
    {
        TrackingBuild.RequireVersion();
        Directory.CreateDirectory(Evidence);
        EditorSceneManager.OpenScene(Scene);
        var staticSheep = GameObject.Find("07_oveja_r04");
        var staticSkin = staticSheep.GetComponentInChildren<SkinnedMeshRenderer>();
        var baked = new Mesh();
        staticSkin.BakeMesh(baked, true);
        var standing = baked.vertices.Select(v => staticSkin.transform.TransformPoint(v)).ToArray();
        float height = standing.Max(v => v.y) - standing.Min(v => v.y);
        ExchangeBuild.Require(Mathf.Abs(height - 1.18f) < .001f, "Standing height " + height);
        var cam = GameObject.Find("CinemaCamera").GetComponent<Camera>();
        ExchangeBuild.SaveCamera(cam, Evidence + "/in_environment.png");
        cam.Render();
        ExchangeBuild.SaveCamera(GameObject.Find("ExteriorPreviewCamera").GetComponent<Camera>(),
                                 Evidence + "/window.png");
        staticSheep.SetActive(false);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(PoseModel));
        Materials(go);
        var animator = go.GetComponent<Animator>();
        if (animator == null)
            animator = go.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        var clip = ExchangeBuild.Clip(PoseModel);
        ExchangeBuild.Require(Mathf.Abs(clip.length - 6) < .0001f, "Aptitude sample duration");
        var graph = PlayableGraph.Create("Sheep import validation");
        graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        var playable = AnimationClipPlayable.Create(graph, clip);
        playable.SetApplyFootIK(false);
        AnimationPlayableOutput.Create(graph, "Sheep", animator).SetSourcePlayable(playable);
        graph.Play();
        var skin = go.GetComponentInChildren<SkinnedMeshRenderer>();
        skin.forceMatrixRecalculationPerRender = true;
        skin.updateWhenOffscreen = true;
        skin.quality = SkinQuality.Bone4;
        var reference = JsonUtility.FromJson<Reference>(File.ReadAllText(Folder + "/07_oveja_r04.json"));
        var result = new Report { triangles = skin.sharedMesh.triangles.Length / 3,
                                  bones = skin.bones.Length, materials = skin.sharedMesh.subMeshCount,
                                  standingHeight = height, minPoseHeight = 100 };
        ExchangeBuild.Require(result.triangles == 612 && result.materials == 2,
                              "Topology/material preservation");
        cam.transform.position = new Vector3(2.2f, 1.6f, -3);
        cam.transform.LookAt(new Vector3(0, .55f, 0));
        cam.usePhysicalProperties = false;
        cam.fieldOfView = 32;
        foreach (var sample in reference.samples)
        {
            playable.SetTime(sample.time);
            graph.Evaluate(0);
            skin.BakeMesh(baked, true);
            var points = baked.vertices.Select(v => skin.transform.TransformPoint(v)).ToArray();
            foreach (var expected in sample.vertices)
                result.maxVertexError =
                    Mathf.Max(result.maxVertexError,
                              points.Min(p => Vector3.Distance(p, ExchangeBuild.Convert(expected))));
            result.minPoseHeight = Mathf.Min(result.minPoseHeight, points.Min(v => v.y));
            result.samples++;
            ExchangeBuild.SaveCamera(cam, Evidence + "/pose_" + sample.label + ".png");
        }
        graph.Destroy();
        ExchangeBuild.Require(result.maxVertexError < .001f, "Skin import error " + result.maxVertexError);
        ExchangeBuild.Require(result.minPoseHeight > -.001f, "Pose below ground " + result.minPoseHeight);
        result.passed = true;
        File.WriteAllText(Evidence + "/checks.json", JsonUtility.ToJson(result, true) + "\n");
        Debug.Log("CHUPACABRAS_SHEEP_OK " + JsonUtility.ToJson(result));
    }
}
