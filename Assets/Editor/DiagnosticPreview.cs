using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Reproducible Editor evidence only: never presented as a phone screenshot.
[InitializeOnLoad]
public static class DiagnosticPreview
{
    private const string Active = "Chupacabras.Capture.Active";
    private const string Output = "Chupacabras.Capture.Output";
    private static double started;
    private static bool requested;

    static DiagnosticPreview()
    {
        EditorApplication.update += Tick;
    }

    public static void Capture()
    {
        // Let the Editor finish startup callbacks before the Play Mode reload.
        EditorApplication.delayCall += BeginCapture;
    }

    private static void BeginCapture()
    {
        string path = "docs/evidencias/editor_diagnostic_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".png";
        SessionState.SetString(Output, path);
        SessionState.SetBool(Active, true);
        EditorSceneManager.OpenScene("Assets/Scenes/02_DetectorSmoke.unity");
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (started == 0) started = EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup - started > 60)
        {
            Debug.LogError("Diagnostic preview timed out.");
            SessionState.SetBool(Active, false);
            EditorApplication.Exit(1);
            return;
        }
        if (!EditorApplication.isPlaying || Time.frameCount < 30) return;
        string path = SessionState.GetString(Output, "");
        if (!requested)
        {
            ScreenCapture.CaptureScreenshot(path);
            requested = true;
        }
        if (!File.Exists(path) || new FileInfo(path).Length == 0) return;
        Debug.Log("CHUPACABRAS_EDITOR_PREVIEW " + path);
        SessionState.SetBool(Active, false);
        EditorApplication.Exit(0);
    }
}
