using System;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AppearancePreview {
  const string Active = "Chupacabras.AppearancePreview";
  static double started, phaseStart, frozen;
  static int phase;
  static bool injected;
  static string output;
  static AppearancePreview() { EditorApplication.update += Tick; }
  public static void Capture() {
    TrackingBuild.RequireVersion();
    Directory.CreateDirectory(AppearanceBuild.Evidence);
    EditorApplication.delayCall += () => {
      EditorSceneManager.OpenScene(AppearanceBuild.Scene);
      UnityEngine.Object.FindFirstObjectByType<TrackingProbe>()
          .syntheticPreview = true;
      SessionState.SetBool(Active, true);
      EditorApplication.ExecuteMenuItem("Window/General/Game");
      EditorApplication.EnterPlaymode();
    };
  }
  static void Tick() {
    if (!SessionState.GetBool(Active, false))
      return;
    if (started == 0)
      started = EditorApplication.timeSinceStartup;
    if (EditorApplication.timeSinceStartup - started > 90) {
      Finish(1, "Timeout");
      return;
    }
    if (!EditorApplication.isPlaying || Time.frameCount < 2)
      return;
    var probe = UnityEngine.Object.FindFirstObjectByType<TrackingProbe>();
    if (probe == null)
      return;
    if (!injected) {
      var fixture = Resources.Load<Texture2D>("AprilTagFixture");
      var small = new Texture2D(640, 480, TextureFormat.RGBA32, false);
      var pixels = Enumerable.Repeat(new Color32(230, 230, 230, 255), 640 * 480)
                       .ToArray();
      var source = fixture.GetPixels32();
      for (int y = 0; y < 128; y++)
        for (int x = 0; x < 128; x++)
          pixels[(y + 176) * 640 + x + 256] =
              source[(y * fixture.height / 128) * fixture.width +
                     x * fixture.width / 128];
      small.SetPixels32(pixels);
      small.Apply();
      probe.syntheticFixture = small;
      injected = true;
      phaseStart = EditorApplication.timeSinceStartup;
      return;
    }
    if (EditorApplication.timeSinceStartup - phaseStart < 2)
      return;
    try {
      var study = probe.appearance;
      bool visible = phase != 1;
      ExchangeBuild.Require(probe.State.Visible == visible,
                            "Tracking state " + phase);
      ExchangeBuild.Require(study.visualRoot.gameObject.activeInHierarchy ==
                                visible,
                            "Both exterior elements visibility");
      ExchangeBuild.Require(study.cinema.enabled == visible,
                            "Cinema visibility");
      if (phase == 1 && frozen == 0) {
        frozen = probe.State.PlaybackSeconds;
        phaseStart = EditorApplication.timeSinceStartup;
        return;
      }
      if (phase == 1)
        ExchangeBuild.Require(Math.Abs(probe.State.PlaybackSeconds - frozen) <
                                  .001,
                              "Clock during loss");
      if (phase == 2)
        ExchangeBuild.Require(probe.State.PlaybackSeconds > frozen,
                              "Resume clock");
      if (output == null) {
        output = AppearanceBuild.Evidence + "/runtime_" + phase + ".png";
        ExchangeBuild.Require(!File.Exists(output), "Preserve capture");
        if (study.cinema.enabled)
          study.cinema.Render();
        var view = GameObject.Find("ARCamera").GetComponent<Camera>();
        var previousRect = view.rect;
        // Evidence target is 960x540; fit its viewport independently of Game
        // View.
        view.rect = CameraGeometry.Fit(view.aspect, 960f / 540);
        ExchangeBuild.SaveCamera(view, output);
        view.rect = previousRect;
        return;
      }
      if (!File.Exists(output))
        return;
      if (++phase == 3) {
        Finish(0, "Acquisition, loss, paused clock, recovery verified in " +
                      "synthetic Editor only");
        return;
      }
      probe.syntheticBlank = phase == 1;
      output = null;
      phaseStart = EditorApplication.timeSinceStartup;
    } catch (Exception e) {
      Finish(1, e.ToString());
    }
  }
  static void Finish(int code, string message) {
    File.WriteAllText(
        AppearanceBuild.Evidence + "/runtime.json",
        JsonUtility.ToJson(new Result { passed = code == 0, detail = message },
                           true));
    Debug.Log("CHUPACABRAS_APPEARANCE_RUNTIME " + message);
    SessionState.SetBool(Active, false);
    EditorApplication.Exit(code);
  }
  [Serializable]
  class Result {
    public bool passed;
    public bool physicalDevice = false;
    public string detail;
  }
}
