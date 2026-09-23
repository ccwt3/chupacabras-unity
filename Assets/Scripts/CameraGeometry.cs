using System;
using UnityEngine;

namespace Chupacabras
{
    public static class CameraGeometry
    {
        // Both arrays use Unity's bottom-left origin. Unmirror BEFORE clockwise rotation.
        public static void Normalize(Color32[] source, Color32[] target, int width, int height,
            int clockwise, bool mirrored)
        {
            if (clockwise != 0 && clockwise != 90 && clockwise != 180 && clockwise != 270)
                throw new ArgumentOutOfRangeException(nameof(clockwise));
            int outputWidth = clockwise % 180 == 0 ? width : height;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int sx = x, sy = mirrored ? height - 1 - y : y;
                int dx = x, dy = y;
                switch (clockwise)
                {
                    case 90: dx = y; dy = width - 1 - x; break;
                    case 180: dx = width - 1 - x; dy = height - 1 - y; break;
                    case 270: dx = height - 1 - y; dy = x; break;
                }
                target[dy * outputWidth + dx] = source[sy * width + sx];
            }
        }

        public static float UprightFov(float sensorVerticalDegrees, int width, int height, int rotation)
            => rotation % 180 == 0 ? sensorVerticalDegrees :
                2 * Mathf.Atan(Mathf.Tan(sensorVerticalDegrees * Mathf.Deg2Rad / 2) * width / height) * Mathf.Rad2Deg;

        public static Rect Fit(float imageAspect, float screenAspect)
        {
            if (screenAspect > imageAspect)
            {
                float w = imageAspect / screenAspect;
                return new Rect((1 - w) / 2, 0, w, 1);
            }
            float h = screenAspect / imageAspect;
            return new Rect(0, (1 - h) / 2, 1, h);
        }
    }

    // No smoothing: preserve raw poses for viability measurements.
    public sealed class TrackingState
    {
        public bool Visible { get; private set; }
        public double LastFrame { get; private set; } = double.NegativeInfinity;
        public double PlaybackSeconds { get; private set; }
        public void Observe(bool valid, double now) { Visible = valid; LastFrame = now; }
        public void Tick(double now, float delta)
        {
            if (now - LastFrame > 0.4) Visible = false;
            if (Visible) PlaybackSeconds += delta;
        }
        public void Suspend() { Visible = false; LastFrame = double.NegativeInfinity; }
    }
}
