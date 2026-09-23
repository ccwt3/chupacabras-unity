using System;
using System.IO;
using System.Linq;
using AprilTag;
using UnityEngine;

public static class MarkerChecks
{
    public static void Run()
    {
        TrackingBuild.RequireVersion();
        CheckPage("a4", .297f);
        CheckPage("carta", .2794f);
    }

    private static void CheckPage(string paper, float pageHeightMeters)
    {
        var page = new Texture2D(2, 2);
        if (!page.LoadImage(File.ReadAllBytes($"marker/03_marker_{paper}_preview.png"))) throw new Exception("PNG de PDF ilegible");
        using var detector = new TagDetector(page.width, page.height, 2);
        detector.ProcessImage(page.GetPixels32(), 60 * Mathf.Deg2Rad, .1f);
        var tags = detector.DetectedTags.ToArray();
        if (tags.Length != 1 || tags[0].ID != 0) throw new Exception("PDF no detecta solo ID 0");
        Vector3 p = tags[0].Position;
        // Paper height varies; the physical detection square stays at 100 mm.
        float expectedZ = pageHeightMeters / (2 * Mathf.Tan(30 * Mathf.Deg2Rad));
        if (Mathf.Abs(p.z - expectedZ) > .003f) throw new Exception("Escala del PDF incorrecta");
        string json = $"{{\"id\":0,\"passed\":true,\"page_width\":{page.width},\"page_height\":{page.height},\"z\":{p.z.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"physical_print\":false}}\n";
        string output = "docs/evidencias/marker_checks_" + paper + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
        File.WriteAllText(output, json);
        Debug.Log("CHUPACABRAS_MARKER_PDF_OK " + json);
        UnityEngine.Object.DestroyImmediate(page);
    }
}
