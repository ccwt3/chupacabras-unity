using System;
using System.IO;
using System.Linq;
using AprilTag;
using Chupacabras;
using UnityEditor;
using UnityEngine;

public static class TrackingChecks
{
    private static int assertions;
    private static void Check(bool value, string message)
    {
        assertions++;
        if (!value) throw new InvalidOperationException(message);
    }
    public static void Run()
    {
        TrackingBuild.RequireVersion(); assertions = 0;
        // Asymmetric 3x2 image: hard-coded expectations catch sign/order mistakes.
        var source = Enumerable.Range(1, 6).Select(i => new Color32((byte)i, 0, 0, 255)).ToArray();
        int[][] expected = { new[] {1,2,3,4,5,6}, new[] {3,6,2,5,1,4}, new[] {6,5,4,3,2,1}, new[] {4,1,5,2,6,3} };
        int[][] flipped = { new[] {4,5,6,1,2,3}, new[] {6,3,5,2,4,1}, new[] {3,2,1,6,5,4}, new[] {1,4,2,5,3,6} };
        foreach (bool mirror in new[] {false, true})
        for (int angle = 0; angle < 360; angle += 90)
        {
            var target = new Color32[6]; CameraGeometry.Normalize(source, target, 3, 2, angle, mirror);
            Check(target.Select(p => (int)p.r).SequenceEqual((mirror ? flipped : expected)[angle / 90]), $"Pixels {angle}/{mirror}");
        }
        Check(Mathf.Abs(CameraGeometry.UprightFov(60, 640, 480, 90) - 75.17818f) < .001f, "Portrait FOV");
        foreach (float screenAspect in new[] {9f/16, 16f/9, 1f})
        {
            var rect = CameraGeometry.Fit(4f/3, screenAspect);
            Check(Mathf.Abs(rect.width * screenAspect / rect.height - 4f/3) < .0001f, "Letterbox projection");
        }
        var state = new TrackingState();
        state.Tick(0, .1f); Check(!state.Visible && state.PlaybackSeconds == 0, "Initial pause");
        state.Observe(true, 1); state.Tick(1.1, .1f); double before = state.PlaybackSeconds;
        state.Observe(false, 1.2); state.Tick(1.3, .1f);
        Check(!state.Visible && state.PlaybackSeconds == before, "Loss pauses immediately");
        state.Observe(true, 2); state.Tick(2.1, .1f);
        Check(state.Visible && state.PlaybackSeconds > before, "Reacquisition continues");
        before = state.PlaybackSeconds; state.Tick(2.5, .4f);
        Check(!state.Visible && state.PlaybackSeconds == before, "Stalled camera hides");
        state.Observe(true, 3); state.Suspend(); state.Tick(3.1, .1f);
        Check(!state.Visible && state.PlaybackSeconds == before, "Background pauses");
        var fixture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/AprilTagFixture.png");
        DetectorSmokeTest.Run(fixture);
        var pixels = fixture.GetPixels32();
        using (var detector = new TagDetector(fixture.width, fixture.height, 2))
        {
            var rotated = new Color32[pixels.Length];
            for (int angle = 0; angle < 360; angle += 90)
            {
                CameraGeometry.Normalize(pixels, rotated, fixture.width, fixture.height, angle, false);
                detector.ProcessImage(rotated, 60 * Mathf.Deg2Rad, .1f);
                var tags = detector.DetectedTags.ToArray();
                Check(tags.Length == 1 && tags[0].ID == 0, "Rotated tag ID " + angle);
                Vector3 p = tags[0].Position;
                // Source 9x9 pixels enlarged to 288px, detection square = 5 cells =160px.
                float expectedZ = 512f / (2 * Mathf.Tan(30 * Mathf.Deg2Rad)) * .1f / 160;
                Check(Mathf.Abs(p.z - expectedZ) < .003f, "Metric size uses 5 cells, not full 9-cell image");
                Quaternion q = tags[0].Rotation;
                Check(Quaternion.Angle(q, Quaternion.Euler(0, 0, -angle)) < 2, "Pose orientation " + angle);
            }
        }
        // Camera capture at 320x240: full tag remains metric with decimation 1.
        var small = new Color32[320 * 240];
        for (int y = 0; y < 240; y++)
        for (int x = 0; x < 320; x++)
            small[y * 320 + x] = x >= 40 && x < 280
                ? pixels[(y * 2 + 16) * 512 + (x - 40) * 2 + 16]
                : new Color32(255, 255, 255, 255);
        using (var detector = new TagDetector(320, 240, 1))
        {
            detector.ProcessImage(small, 60 * Mathf.Deg2Rad, .1f);
            var tags = detector.DetectedTags.ToArray();
            Check(tags.Length == 1 && tags[0].ID == 0, "Small camera tag ID");
            float expectedZ = 240f / (2 * Mathf.Tan(30 * Mathf.Deg2Rad)) * .1f / 80;
            Check(Mathf.Abs(tags[0].Position.z - expectedZ) < .003f, "Small camera metric depth");
            Array.Fill(small, new Color32(255, 255, 255, 255));
            detector.ProcessImage(small, 60 * Mathf.Deg2Rad, .1f);
            Check(!detector.DetectedTags.Any(), "Small blank camera has no detection");
        }
        Directory.CreateDirectory("docs/evidencias");
        string output = "docs/evidencias/tracking_checks_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
        File.WriteAllText(output, $"{{\"passed\":true,\"assertions\":{assertions},\"unity\":\"{Application.unityVersion}\",\"physical_device\":false}}\n");
        Debug.Log($"CHUPACABRAS_TRACKING_CHECKS_OK {assertions} assertions " + output);
    }
}
