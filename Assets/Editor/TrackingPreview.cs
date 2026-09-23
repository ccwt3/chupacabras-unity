using System;
using System.IO;
using Chupacabras;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class TrackingPreview
{
    private const string Active = "Chupacabras.TrackingCapture";
    private static double started, phaseTime;
    private static int phase;
    private static bool requested;
    private static string path;
    private static double pausedClock;
    static TrackingPreview() { EditorApplication.update += Tick; }
    public static void Capture()
    {
        TrackingBuild.RequireVersion();
        EditorApplication.delayCall += () =>
        {
            EditorSceneManager.OpenScene(TrackingBuild.ScenePath);
            UnityEngine.Object.FindFirstObjectByType<TrackingProbe>().syntheticPreview = true;
            SessionState.SetBool(Active, true);
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.EnterPlaymode();
        };
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (started == 0) started = EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup - started > 90) { Finish(1, "Timeout"); return; }
        if (!EditorApplication.isPlaying || Time.frameCount < 30) return;
        var probe = UnityEngine.Object.FindFirstObjectByType<TrackingProbe>();
        if (probe == null) return;
        if (phaseTime == 0) phaseTime = EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup - phaseTime < 1) return;
        bool shouldTrack = phase != 1;
        if (probe.State.Visible != shouldTrack) { Finish(1, "Unexpected visibility"); return; }
        if (phase == 1 && pausedClock > 0 && Math.Abs(probe.State.PlaybackSeconds - pausedClock) > .001)
        { Finish(1, "Playback advanced while lost"); return; }
        if (!requested)
        {
            if (phase == 1) pausedClock = probe.State.PlaybackSeconds;
            path = "docs/evidencias/editor_tracking_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + phase + ".png";
            ScreenCapture.CaptureScreenshot(path); requested = true; return;
        }
        if (!File.Exists(path) || new FileInfo(path).Length == 0) return;
        Debug.Log("CHUPACABRAS_SYNTHETIC_CAPTURE " + path);
        phase++; requested = false; phaseTime = EditorApplication.timeSinceStartup;
        if (phase == 3)
        {
            if (probe.State.PlaybackSeconds <= pausedClock) { Finish(1, "Playback did not resume"); return; }
            Finish(0, "acquired/lost/recovered; synthetic Editor only"); return;
        }
        probe.syntheticBlank = phase == 1;
    }
    private static void Finish(int code, string message)
    {
        Debug.Log("CHUPACABRAS_PREVIEW " + message);
        SessionState.SetBool(Active, false); EditorApplication.Exit(code);
    }
}
