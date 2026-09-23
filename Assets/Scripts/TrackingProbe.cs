using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using AprilTag;
using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Chupacabras
{
    public sealed class TrackingProbe : MonoBehaviour
    {
        public Shader unlitShader;
        public Shader litShader;
#if UNITY_EDITOR
        public bool syntheticPreview;
        public bool syntheticBlank;
#endif
        public readonly TrackingState State = new TrackingState();
        public string Status { get; private set; } = "Iniciando cámara…";
        public string EvidenceDirectory { get; private set; }
        public float SensorFov = 60; // Adjustable hypothesis, NEVER a device calibration.
        public float TagMeters = 0.10f;
        private Camera view, cinema;
        private Transform anchor, spinner, backdrop;
        private Material backdropMaterial;
        private RenderTexture renderTexture;
        private GameObject window;
        private WebCamTexture webcam;
        private Texture2D image;
        private TagDetector detector;
        private Color32[] raw, upright;
        private StreamWriter telemetry;
        private readonly System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        private int rawWidth, rawHeight, rotation, rearIndex, processedFrames;
        private bool mirrored, opening, paused, ready, rtEnabled = true;
        private double lastProcessed, nextSample;
        private float detectMs, imageMs, frameMs;
        private Vector3 position;
        private Quaternion poseRotation = Quaternion.identity;
        private string cameraName = "none";
        private string phase = "inicio";

        private void Start()
        {
            Application.targetFrameRate = 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            #if UNITY_EDITOR
            EvidenceDirectory = Path.GetFullPath(Path.Combine("docs/evidencias",
                "editor_probe_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff")));
#else
            EvidenceDirectory = Path.Combine(Application.persistentDataPath,
                "probe_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
#endif
            Directory.CreateDirectory(EvidenceDirectory);
            telemetry = new StreamWriter(Path.Combine(EvidenceDirectory, "tracking.csv"));
            telemetry.WriteLine("seconds,phase,visible,playback_s,frame_ms,image_ms,detect_ms,rt,width,height,rotation,mirrored,sensor_fov,tag_m,x,y,z,qx,qy,qz,qw,processed_frames");
            CreateView();
#if UNITY_EDITOR
            if (syntheticPreview) { cameraName = "SYNTHETIC_EDITOR"; ready = true; return; }
#endif
            try
            {
                var result = DetectorSmokeTest.Run(Resources.Load<Texture2D>("AprilTagFixture"));
                result.limitation = "Imagen incluida; no valida marcador físico, escala ni calibración.";
                string json = JsonUtility.ToJson(result, true);
                File.WriteAllText(Path.Combine(EvidenceDirectory, "detector-device.json"), json);
                Debug.Log("CHUPACABRAS_DEVICE_DETECTOR_OK " + json);
            }
            catch (Exception error)
            {
                Status = "Falló la comprobación del detector; consulta el registro.";
                Debug.LogException(error);
                return;
            }
            StartCoroutine(OpenCamera());
        }

        private Material Material(Shader shader, Color color)
        {
            if (shader == null) throw new InvalidOperationException("Falta shader de prueba.");
            var material = new Material(shader);
            material.SetColor("_BaseColor", color);
            return material;
        }

        private Transform Primitive(string name, PrimitiveType type, Transform parent,
            Vector3 location, Vector3 scale, Color color, int layer = 0, bool lit = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name; go.layer = layer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = location; go.transform.localScale = scale;
            Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = Material(lit ? litShader : unlitShader, color);
            return go.transform;
        }

        private void CreateView()
        {
            var clear = new GameObject("LetterboxClear").AddComponent<Camera>();
            clear.cullingMask = 0; clear.depth = -2;
            clear.clearFlags = CameraClearFlags.SolidColor; clear.backgroundColor = Color.black;
            clear.GetUniversalAdditionalCameraData();
            view = new GameObject("ARCamera").AddComponent<Camera>();
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = Color.black;
            view.nearClipPlane = .01f; view.farClipPlane = 10;
            view.cullingMask = 1; view.GetUniversalAdditionalCameraData();
            backdrop = Primitive("CameraImage", PrimitiveType.Quad, null, new Vector3(0, 0, 5),
                Vector3.one, Color.white);
            backdropMaterial = backdrop.GetComponent<Renderer>().sharedMaterial;
            anchor = new GameObject("Tag0_100mm").transform;
            Primitive("Cube_50mm", PrimitiveType.Cube, anchor, new Vector3(0, 0, -.025f),
                Vector3.one * .05f, new Color(.35f, .7f, .68f), lit: true);
            // A thin cross marks the physical detection square's center and axes.
            Primitive("X_100mm", PrimitiveType.Cube, anchor, new Vector3(0, 0, -.001f),
                new Vector3(.10f, .002f, .001f), new Color(.8f, .35f, .25f));
            Primitive("Y_100mm", PrimitiveType.Cube, anchor, new Vector3(0, 0, -.002f),
                new Vector3(.002f, .10f, .001f), new Color(.3f, .55f, .85f));
            var light = new GameObject("ProbeLight").AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.5f;
            light.transform.rotation = Quaternion.Euler(40, -30, 0);
            light.shadows = LightShadows.None;
            renderTexture = new RenderTexture(512, 288, 16) { name = "ProbeRT_512x288" };
            renderTexture.Create();
            cinema = new GameObject("LoadProbeCamera").AddComponent<Camera>();
            cinema.transform.position = new Vector3(10, 0, -3);
            cinema.clearFlags = CameraClearFlags.SolidColor;
            cinema.backgroundColor = new Color(.05f, .08f, .15f);
            cinema.cullingMask = 1 << 8; cinema.targetTexture = renderTexture;
            cinema.depth = -1; cinema.GetUniversalAdditionalCameraData();
            spinner = Primitive("LoadProbeCube", PrimitiveType.Cube, null, new Vector3(10, 0, 0),
                Vector3.one, new Color(.7f, .55f, .3f), 8, true);
            window = Primitive("RenderTextureTest", PrimitiveType.Quad, anchor,
                new Vector3(.13f, 0, -.001f), new Vector3(.14f, .07875f, 1), Color.white).gameObject;
            window.GetComponent<Renderer>().sharedMaterial.SetTexture("_BaseMap", renderTexture);
            anchor.gameObject.SetActive(false); cinema.enabled = false;
        }

        private IEnumerator OpenCamera()
        {
            if (opening || paused) yield break;
            opening = true; ready = false;
            Status = "Esperando permiso de cámara…";
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                bool answered = false;
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += _ => answered = true;
                callbacks.PermissionDenied += _ => answered = true;
                Permission.RequestUserPermission(Permission.Camera, callbacks);
                double deadline = Time.realtimeSinceStartupAsDouble + 45;
                while (!answered && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Status = "Cámara denegada. Autorízala en Ajustes y pulsa Reintentar.";
                opening = false; yield break;
            }
#endif
            var devices = WebCamTexture.devices.Where(d => !d.isFrontFacing)
                .OrderBy(d => d.kind == WebCamKind.WideAngle ? 0 : 1).ToArray();
            if (devices.Length == 0)
            {
                Status = "No hay cámara trasera disponible."; opening = false; yield break;
            }
            rearIndex %= devices.Length;
            cameraName = devices[rearIndex].name;
            File.WriteAllText(Path.Combine(EvidenceDirectory, "device.txt"),
                $"UTC {DateTime.UtcNow:O}\nUnity {Application.unityVersion}\n{SystemInfo.deviceModel}\n{SystemInfo.operatingSystem}\n" +
                $"Camera {cameraName}\nRequested 320x240 @15\n" +
                string.Join("\n", devices.Select(d => $"{d.name}: {d.kind}")) +
                "\nFOV 60 degrees is provisional; physical calibration pending.\n");
            webcam = new WebCamTexture(cameraName, 320, 240, 15);
            webcam.Play(); Status = "Abriendo cámara trasera…";
            double timeout = Time.realtimeSinceStartupAsDouble + 15;
            while ((webcam.width <= 16 || !webcam.didUpdateThisFrame) && Time.realtimeSinceStartupAsDouble < timeout)
                yield return null;
            ready = webcam.width > 16 && webcam.didUpdateThisFrame;
            Status = ready ? "Busca el marcador ID 0." : "Sin imágenes de cámara. Pulsa Reintentar.";
            if (!ready) CloseCamera();
            opening = false;
        }

        private void Update()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            frameMs = Mathf.Lerp(frameMs, Time.unscaledDeltaTime * 1000, .05f);
            // WebCamTexture is requested at 15 Hz; process each fresh frame.
            // A second 1/15 time gate skips frames when the 30 Hz display runs slightly fast.
            if (!paused && ready)
            {
                try
                {
#if UNITY_EDITOR
                    if (syntheticPreview)
                    {
                        if (now - lastProcessed >= 1.0 / 15)
                        {
                            timer.Restart();
                            var fixture = Resources.Load<Texture2D>("AprilTagFixture");
                            Process(fixture.GetPixels32(), fixture.width, fixture.height, 0, false, now, syntheticBlank);
                        }
                    }
                    else
#endif
                    if (webcam != null && webcam.didUpdateThisFrame && webcam.width > 16)
                    {
                        timer.Restart();
                        if (raw == null || raw.Length != webcam.width * webcam.height) raw = new Color32[webcam.width * webcam.height];
                        webcam.GetPixels32(raw);
                        Process(raw, webcam.width, webcam.height, webcam.videoRotationAngle, webcam.videoVerticallyMirrored, now);
                    }
                }
                catch (Exception error)
                {
                    Debug.LogException(error); Status = "Error de cámara/detector. Consulta el registro.";
                    CloseCamera();
                }
            }
            State.Tick(now, Time.unscaledDeltaTime);
            anchor.gameObject.SetActive(State.Visible);
            cinema.enabled = State.Visible && rtEnabled;
            window.SetActive(rtEnabled);
            spinner.rotation = Quaternion.Euler(20, (float)State.PlaybackSeconds * 45, 15);
            if (ready && !State.Visible && now - lastProcessed > .4) Status = "Sin imagen reciente; seguimiento pausado.";
            if (image != null) ConfigureProjection();
            if (now >= nextSample) { WriteSample(now); nextSample = now + .1; }
        }

        private void Process(Color32[] pixels, int width, int height, int angle, bool flip, double now, bool blank = false)
        {
            if (image == null || rawWidth != width || rawHeight != height || rotation != angle || mirrored != flip)
            {
                State.Suspend(); detector?.Dispose(); if (image != null) Destroy(image);
                rawWidth = width; rawHeight = height; rotation = angle; mirrored = flip;
                int w = angle % 180 == 0 ? width : height, h = angle % 180 == 0 ? height : width;
                upright = new Color32[w * h]; image = new Texture2D(w, h, TextureFormat.RGBA32, false);
                // Keep the small capture at full detector resolution; handle camera fallback too.
                detector = new TagDetector(w, h, Mathf.Min(w, h) <= 240 ? 1 : 2);
                backdropMaterial.SetTexture("_BaseMap", image);
                Debug.Log($"CHUPACABRAS_CAMERA {cameraName} {width}x{height} rotation={angle} mirrored={flip}");
            }
            CameraGeometry.Normalize(pixels, upright, width, height, angle, flip);
            if (blank) Array.Fill(upright, new Color32(255, 255, 255, 255));
            image.SetPixels32(upright); image.Apply(false, false);
            imageMs = (float)timer.Elapsed.TotalMilliseconds;
            ConfigureProjection();
            timer.Restart();
            detector.ProcessImage(upright, view.fieldOfView * Mathf.Deg2Rad, TagMeters);
            detectMs = (float)timer.Elapsed.TotalMilliseconds; timer.Stop();
            bool valid = false;
            foreach (var tag in detector.DetectedTags)
            {
                if (tag.ID != 0) continue;
                Vector3 p = tag.Position; Quaternion q = tag.Rotation;
                if (!float.IsFinite(p.x) || !float.IsFinite(p.y) || !float.IsFinite(p.z) || p.z <= .01f || p.z >= 4 ||
                    !float.IsFinite(q.x) || !float.IsFinite(q.y) || !float.IsFinite(q.z) || !float.IsFinite(q.w)) continue;
                position = p; poseRotation = q;
                anchor.SetPositionAndRotation(p, q); valid = true; break;
            }
            if (State.Visible != valid) Debug.Log("CHUPACABRAS_TRACK " + (valid ? "acquired" : "lost"));
            State.Observe(valid, now); lastProcessed = now; processedFrames++;
            Status = valid ? "ID 0 visible · cubo 5 cm · escala por comprobar" : "Marcador ausente · oculto y pausado";
        }

        private void ConfigureProjection()
        {
            view.fieldOfView = CameraGeometry.UprightFov(SensorFov, rawWidth, rawHeight, rotation);
            view.aspect = (float)image.width / image.height;
            view.rect = CameraGeometry.Fit(view.aspect, (float)Screen.width / Screen.height);
            float h = 10 * Mathf.Tan(view.fieldOfView * Mathf.Deg2Rad / 2);
            backdrop.localScale = new Vector3(h * view.aspect, h, 1);
        }

        private void WriteSample(double now)
        {
            if (telemetry == null) return;
            telemetry.WriteLine(FormattableString.Invariant($"{now:F4},{phase},{State.Visible},{State.PlaybackSeconds:F4},{frameMs:F3},{imageMs:F3},{detectMs:F3},{rtEnabled},{rawWidth},{rawHeight},{rotation},{mirrored},{SensorFov:F2},{TagMeters:F4},{position.x:F6},{position.y:F6},{position.z:F6},{poseRotation.x:F6},{poseRotation.y:F6},{poseRotation.z:F6},{poseRotation.w:F6},{processedFrames}"));
            telemetry.Flush();
        }

        private void OnGUI()
        {
            float scale = Mathf.Max(1, Mathf.Min(Screen.width / 480f, Screen.height / 600f));
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float w = Screen.width / scale, h = Screen.height / scale;
            GUI.Box(new Rect(0, 0, w, 95), "");
            var label = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 14 };
            GUI.Label(new Rect(12, 6, w - 24, 28), "CHUPACABRAS · prueba 03 · " + phase, label);
            GUI.Label(new Rect(12, 32, w - 24, 52), Status + $"\n{frameMs:F1} ms/frame · detector {detectMs:F1} ms · reloj {State.PlaybackSeconds:F1} s", label);
            GUI.Box(new Rect(0, h - 174, w, 174), "");
            GUI.Label(new Rect(12, h - 168, w - 24, 24), $"FOV provisional {SensorFov:F1}° · tag {TagMeters * 100:F1} cm", label);
            float fov = GUI.HorizontalSlider(new Rect(12, h - 140, w - 24, 20), SensorFov, 30, 100);
            if (Mathf.Abs(fov - SensorFov) > .01f) { SensorFov = fov; State.Suspend(); }
            if (GUI.Button(new Rect(12, h - 111, 95, 32), "Reintentar")) RestartCamera(false);
            if (GUI.Button(new Rect(114, h - 111, 90, 32), "Otra trasera")) RestartCamera(true);
            if (GUI.Button(new Rect(211, h - 111, 90, 32), rtEnabled ? "RT: sí" : "RT: no")) rtEnabled = !rtEnabled;
            if (GUI.Button(new Rect(12, h - 70, 95, 32), "Captura"))
            {
                string stamp = DateTime.UtcNow.ToString("HHmmss_fff");
                ScreenCapture.CaptureScreenshot(Path.Combine(EvidenceDirectory, stamp + "_screen.png"));
                if (image != null) File.WriteAllBytes(Path.Combine(EvidenceDirectory, stamp + "_camera.png"), image.EncodeToPNG());
                WriteSample(Time.realtimeSinceStartupAsDouble);
            }
            if (GUI.Button(new Rect(114, h - 70, 187, 32), "Marcar siguiente prueba"))
            {
                string[] phases = { "inicio", "fijo", "cerca", "lejos", "inclinar", "ocultar", "recuperar", "rendimiento" };
                phase = phases[(Array.IndexOf(phases, phase) + 1) % phases.Length];
            }
            GUI.Label(new Rect(12, h - 32, w - 24, 26), "Prueba técnica; escala y rendimiento en evaluación.", label);
        }

        private void RestartCamera(bool next)
        {
            if (opening || paused) return;
            CloseCamera(); if (next) rearIndex++;
            StartCoroutine(OpenCamera());
        }
        private void CloseCamera()
        {
            ready = false; State.Suspend();
            if (webcam != null) { webcam.Stop(); Destroy(webcam); webcam = null; }
            detector?.Dispose(); detector = null;
            if (image != null) { Destroy(image); image = null; }
        }
        private void OnApplicationPause(bool value)
        {
            paused = value;
            if (value) { StopAllCoroutines(); opening = false; CloseCamera(); telemetry?.Flush(); }
            else if (view != null) StartCoroutine(OpenCamera());
        }
        private void OnDestroy()
        {
            CloseCamera(); telemetry?.Dispose();
            if (renderTexture != null) { renderTexture.Release(); Destroy(renderTexture); }
        }
    }
}
