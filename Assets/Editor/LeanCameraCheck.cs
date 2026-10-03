using System;
using System.IO;
using System.Linq;
using Chupacabras;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LeanCameraCheck {
  public static void Run() {
    TrackingBuild.RequireVersion();
    EditorSceneManager.OpenScene(
        "Assets/Scenes/08_chupacabras_demacrado_r01_contexto_r02.unity");
    var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
    var cam = ExchangeBuild.Find(player.animator.gameObject, "CinemaCamera")
                  .GetComponent<Camera>();
    var renderers =
        UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
            .Where(r => r.gameObject.layer == 8 &&
                        r.gameObject.activeInHierarchy)
            .ToArray();
    var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    black.SetColor("_BaseColor", Color.black);
    var white = new Material(black);
    white.SetColor("_BaseColor", Color.white);
    foreach (var r in renderers)
      r.sharedMaterials =
          r.sharedMaterials
              .Select(
                  _ => r.name.StartsWith("Sheep_") ? white : black)
              .ToArray();
    var rt = new RenderTexture(320, 180, 24);
    var image = new Texture2D(320, 180, TextureFormat.RGB24, false);
    cam.targetTexture = rt;
    cam.backgroundColor = Color.black;
    int Count(float time, Vector3 pos) {
      player.Evaluate(time);
      cam.transform.position = pos;
      cam.transform.LookAt(new Vector3(0, .95f, 0));
      cam.Render();
      var old = RenderTexture.active;
      RenderTexture.active = rt;
      image.ReadPixels(new Rect(0, 0, 320, 180), 0, 0);
      image.Apply();
      RenderTexture.active = old;
      return image.GetPixels32().Count(p =>
                                           p.r > 200 && p.g > 200 && p.b > 200);
    }
    int best = int.MaxValue;
    Vector3 position = Vector3.zero;
    var lines =
        new System.Collections.Generic.List<string> { "x,z,y,total_pixels" };
    for (int depth = 3; depth <= 6; depth++)
      for (int z = 16; z <= 34; z += 2)
        for (int x = -8; x <= 8; x += 2) {
          var pos = new Vector3(x / 10f, z / 10f, -depth);
          int sum =
              new[] { 21.2f, 21.4f, 21.6f, 21.8f, 22, 23, 24.966667f }.Sum(
                  t => Count(t, pos));
          lines.Add($"{pos.x},{pos.y},{pos.z},{sum}");
          if (sum < best) {
            best = sum;
            position = pos;
          }
        }
    var dir = "docs/evidencias/2026-10-02_demacrado_camera_r02";
    Directory.CreateDirectory(dir);
    File.WriteAllLines(dir + "/search.csv", lines);
    int total = 0;
    for (int f = 636; f < 750; f++)
      total += Count(f / 30f, position);
    File.WriteAllText(
        dir + "/result.txt",
        $"Best position Unity {position}; sampled pixels {best}; full range pixels {total}\n");
    Debug.Log($"LEAN_CAMERA {position} pixels {total}");
    rt.Release();
  }

  // Independent full-resolution check of the saved FBX scene, without moving
  // its camera.
  public static void Verify() {
    TrackingBuild.RequireVersion();
    EditorSceneManager.OpenScene(
        "Assets/Scenes/09_chupacabras_demacrado_r02_contexto_r03.unity");
    var player = UnityEngine.Object.FindFirstObjectByType<SequencePreview>();
    var cam = ExchangeBuild.Find(player.animator.gameObject, "CinemaCamera")
                  .GetComponent<Camera>();
    var renderers =
        UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
            .Where(r => r.gameObject.layer == 8 &&
                        r.gameObject.activeInHierarchy)
            .ToArray();
    var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    black.SetColor("_BaseColor", Color.black);
    var white = new Material(black);
    white.SetColor("_BaseColor", Color.white);
    var rt = new RenderTexture(960, 540, 24);
    var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
    cam.targetTexture = rt;
    cam.backgroundColor = Color.black;
    void Mask(bool sheep) {
      foreach (var r in renderers)
        r.sharedMaterials = r.sharedMaterials
                                .Select(
                                    _ => (sheep ? r.name.StartsWith("Sheep_")
                                                : r.name.StartsWith("Chupa_"))
                                             ? white
                                             : black)
                                .ToArray();
    }
    int Count(float time) {
      player.Evaluate(time);
      cam.Render();
      var old = RenderTexture.active;
      RenderTexture.active = rt;
      image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
      image.Apply();
      RenderTexture.active = old;
      return image.GetPixels32().Count(p =>
                                           p.r > 200 && p.g > 200 && p.b > 200);
    }
    Mask(true);
    var sheepCounts =
        Enumerable.Range(636, 114).Select(f => Count(f / 30f)).ToArray();
    foreach (var r in renderers.Where(r => r.name.StartsWith("Chupa_")))
      r.enabled = false;
    int unobstructed = Count(22.5f);
    foreach (var r in renderers.Where(r => r.name.StartsWith("Chupa_")))
      r.enabled = true;
    Mask(false);
    var creatureCounts =
        Enumerable.Range(450, 150).Select(f => Count(f / 30f)).ToArray();
    var dir = "docs/evidencias/2026-10-02_demacrado_fullres";
    Directory.CreateDirectory(dir);
    File.WriteAllText(
        dir + "/checks.json",
        "{\n  \"resolution\":[960,540],\n  \"sheepPixelsByFrame\":[" +
            string.Join(",", sheepCounts) +
            "],\n  \"creaturePixelsByFrame\":[" +
            string.Join(",", creatureCounts) +
            "],\n  \"unobstructedSheepPixels\":" + unobstructed +
            ",\n  \"physicalDevice\":false\n}\n");
    rt.Release();
    ExchangeBuild.Require(sheepCounts.All(n => n == 0),
                          "Sheep visible at 960x540");
    ExchangeBuild.Require(creatureCounts.All(n => n == 0),
                          "Creature visible at 960x540");
    ExchangeBuild.Require(unobstructed > 100,
                          "Occlusion control: sheep outside frame or disabled");
    Debug.Log("LEAN_FULLRES_OK 264 frames; positive control " + unobstructed);
  }
}
