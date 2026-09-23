using System;
using System.IO;
using Chupacabras;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ProjectBuild
{
    private const string ScenePath = "Assets/Scenes/02_DetectorSmoke.unity";

    public static void ConfigureAndVerify()
    {
        RequireVersion();
        Directory.CreateDirectory("Assets/Settings");
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("docs/evidencias");

        var textureImporter = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/AprilTagFixture.png");
        textureImporter.isReadable = true;
        textureImporter.mipmapEnabled = false;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.SaveAndReimport();

        const string rendererPath = "Assets/Settings/MobileRenderer.asset";
        const string pipelinePath = "Assets/Settings/MobileURP.asset";
        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, rendererPath);
        }
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.msaaSampleCount = 1;
            pipeline.supportsHDR = false;
            pipeline.shadowDistance = 10;
            AssetDatabase.CreateAsset(pipeline, pipelinePath);
        }
        GraphicsSettings.defaultRenderPipeline = pipeline;
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipeline;
        }
        QualitySettings.vSyncCount = 0;

        // Scene generation is one-time; subsequent builds preserve authored changes.
        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("DiagnosticCamera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.085f);
            camera.GetUniversalAdditionalCameraData();
            new GameObject("DetectorDiagnostic").AddComponent<DiagnosticScreen>();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.companyName = "Chupacabras";
        PlayerSettings.productName = "Chupacabras — Prueba técnica";
        PlayerSettings.bundleVersion = "0.0.1";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.chupacabras.ar.probe");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
        // ARM64 is the candidate's only Android binary, not a claim about the G20 ABI.
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        EditorUserBuildSettings.buildAppBundle = false;
        AssetDatabase.SaveAssets();
        VerifyDetector();
    }

    public static void VerifyDetector()
    {
        RequireVersion();
        var fixture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/AprilTagFixture.png");
        var result = DetectorSmokeTest.Run(fixture);
        Directory.CreateDirectory("docs/evidencias");
        File.WriteAllText("docs/evidencias/detector-editor.json", JsonUtility.ToJson(result, true) + "\n");
        Debug.Log("CHUPACABRAS_EDITOR_DETECTOR_OK " + JsonUtility.ToJson(result));
    }

    public static void BuildAndroid()
    {
        RequireVersion();
        string output = Environment.GetEnvironmentVariable("CHUPACABRAS_APK");
        if (string.IsNullOrWhiteSpace(output))
            output = "builds/android/02_detector_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".apk";
        if (File.Exists(output)) throw new IOException("No se sobrescribe una APK existente: " + output);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath }, locationPathName = output,
            target = BuildTarget.Android, options = BuildOptions.Development
        });
        File.WriteAllText(output + ".build.txt", $"Unity: {Application.unityVersion}\n" +
            $"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\n" +
            $"Warnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\n" +
            $"Duration: {report.summary.totalTime}\nABI: arm64-v8a PROVISIONAL, G20 pending\n");
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Falló la compilación Android: " + report.summary.result);
        Debug.Log("CHUPACABRAS_APK_OK " + output);
    }

    private static void RequireVersion()
    {
        if (Application.unityVersion != "6000.3.22f1")
            throw new InvalidOperationException("Editor requerido: Unity 6000.3.22f1.");
    }
}
