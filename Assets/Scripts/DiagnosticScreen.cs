using System;
using System.IO;
using UnityEngine;

namespace Chupacabras
{
    public sealed class DiagnosticScreen : MonoBehaviour
    {
        private string status = "Comprobando detector…";
        private Texture2D fixture;

        private void Start()
        {
            Application.targetFrameRate = 30;
            fixture = Resources.Load<Texture2D>("AprilTagFixture");
            try
            {
                var result = DetectorSmokeTest.Run(fixture);
                string json = JsonUtility.ToJson(result, true);
#if UNITY_EDITOR
                string resultPath = "docs/evidencias/detector-playmode.json";
#else
                string resultPath = Path.Combine(Application.persistentDataPath, "detector-smoke.json");
#endif
                File.WriteAllText(resultPath, json);
                Debug.Log("CHUPACABRAS_DETECTOR_OK " + json);
                status = "Detector cargado: ID 0 reconocido. Imagen blanca: sin detecciones.";
            }
            catch (Exception exception)
            {
                status = "Falló la prueba del detector. El detalle quedó en el registro.";
                Debug.LogException(exception);
            }
        }

        private void OnGUI()
        {
            float scale = Mathf.Max(1, Screen.height / 600f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale;
            GUI.Label(new Rect(24, 20, width - 48, 35), "CHUPACABRAS — Prueba técnica 02");
            var wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUI.Label(new Rect(24, 60, width - 48, 60), status, wrapped);
            if (fixture != null)
                GUI.DrawTexture(new Rect(24, 130, 240, 240), fixture, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(24, 390, width - 48, 100),
                "Prueba con imagen incluida. Cámara y seguimiento físico aún pendientes.\n" +
                "Esta pantalla no reproduce el corto ni valida AR en el teléfono.", wrapped);
        }
    }
}
