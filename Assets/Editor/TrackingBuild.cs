using System;
using System.IO;
using Chupacabras;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TrackingBuild
{
    public const string ScenePath = "Assets/Scenes/03_TrackingProbe.unity";
    public static void RequireVersion()
    {
        if (Application.unityVersion != "6000.3.22f1") throw new InvalidOperationException("Unity 6000.3.22f1 requerido.");
    }
    public static void ConfigureAndVerify()
    {
        RequireVersion();
        TrackingChecks.Run();
        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var probe = new GameObject("TrackingProbe").AddComponent<TrackingProbe>();
            probe.unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            probe.litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (probe.unlitShader == null || probe.litShader == null) throw new InvalidOperationException("Faltan shaders URP.");
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        PlayerSettings.productName = "Chupacabras — Seguimiento";
        PlayerSettings.bundleVersion = "0.0.2";
        PlayerSettings.Android.bundleVersionCode = 2;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.chupacabras.ar.trackingprobe");
        PlayerSettings.Android.forceInternetPermission = false;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        // Keep IL2CPP, SDKs, ARM64 and GLES3 already checked in step 2.
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }
    public static void BuildAndroid()
    {
        RequireVersion();
        string output = "builds/android/03_tracking_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".apk";
        if (File.Exists(output)) throw new IOException("APK ya existe: " + output);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath }, locationPathName = output,
            target = BuildTarget.Android, options = BuildOptions.Development
        });
        File.WriteAllText(output + ".build.txt", $"Unity: {Application.unityVersion}\n" +
            $"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\n" +
            $"Bytes: {report.summary.totalSize}\nDuration: {report.summary.totalTime}\nABI: arm64-v8a PROVISIONAL, G20 pending\n");
        if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Falló build de seguimiento.");
        Debug.Log("CHUPACABRAS_TRACKING_APK_OK " + output);
    }
}
