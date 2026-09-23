using System;
using System.Linq;
using AprilTag;
using UnityEngine;

namespace Chupacabras
{
    // Shared by the Editor check and the APK. This does not exercise a camera.
    public static class DetectorSmokeTest
    {
        [Serializable]
        public sealed class Result
        {
            public string unity;
            public string platform;
            public string family = "tagStandard41h12";
            public int id;
            public int detections;
            public int blankDetections;
            public Vector3 position;
            public Quaternion rotation;
            public bool passed;
            public string limitation = "Imagen sintética; cámara, proyección y teléfono pendientes";
        }

        public static Result Run(Texture2D fixture)
        {
            if (fixture == null || !fixture.isReadable)
                throw new InvalidOperationException("Falta la imagen legible de prueba.");

            using var detector = new TagDetector(fixture.width, fixture.height, 2);
            // The pinned implementation derives focal length from image HEIGHT,
            // and expects radians, despite the upstream README saying otherwise.
            detector.ProcessImage(fixture.GetPixels32(), 60 * Mathf.Deg2Rad, 0.10f);
            var tags = detector.DetectedTags.ToArray();
            if (tags.Length != 1 || tags[0].ID != 0)
                throw new InvalidOperationException($"Se esperaba solo ID 0; detectados: {tags.Length}.");

            var tag = tags[0];
            Vector3 position = tag.Position;
            Quaternion rotation = tag.Rotation;
            if (!Finite(position.x) || !Finite(position.y) || !Finite(position.z) || position.z <= 0 ||
                !Finite(rotation.x) || !Finite(rotation.y) || !Finite(rotation.z) || !Finite(rotation.w))
                throw new InvalidOperationException("La pose sintética no es finita o está detrás de la cámara.");

            var white = new Color32[fixture.width * fixture.height];
            Array.Fill(white, new Color32(255, 255, 255, 255));
            detector.ProcessImage(white, 60 * Mathf.Deg2Rad, 0.10f);
            int blankCount = detector.DetectedTags.Count();
            if (blankCount != 0)
                throw new InvalidOperationException("El detector retuvo una detección al procesar blanco.");

            return new Result
            {
                unity = Application.unityVersion, platform = Application.platform.ToString(),
                id = tag.ID, detections = tags.Length, blankDetections = blankCount,
                position = position, rotation = rotation, passed = true
            };
        }

        private static bool Finite(float number) => !float.IsNaN(number) && !float.IsInfinity(number);
    }
}
