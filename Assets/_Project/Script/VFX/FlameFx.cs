using UnityEngine;

namespace MeowStudio.VFX
{
    /// <summary>
    /// Bits every flame script shares: one looping noise table and one camera resolve
    /// per frame, so a room full of candles pays for them once instead of per instance.
    /// </summary>
    public static class FlameFx
    {
        private const int NoiseSamples = 256;
        private static readonly float[] NoiseTable = BuildNoise();

        private static Camera viewer;
        private static int viewerFrame = -1;

        // Integer harmonics only, so the table loops seamlessly and a phase that runs
        // forever never hits a discontinuity.
        private static float[] BuildNoise()
        {
            var table = new float[NoiseSamples];
            for (int i = 0; i < NoiseSamples; i++)
            {
                float t = i * 2f * Mathf.PI / NoiseSamples;
                table[i] = Mathf.Sin(t) * 0.50f
                         + Mathf.Sin(t * 2f + 1.7f) * 0.28f
                         + Mathf.Sin(t * 3f + 4.1f) * 0.16f
                         + Mathf.Sin(t * 5f + 2.3f) * 0.09f
                         + Mathf.Sin(t * 8f + 5.9f) * 0.05f;
            }
            return table;
        }

        /// <summary>Smooth pseudo-noise in roughly -1..1. One array read plus a lerp.</summary>
        public static float Sample(float phase)
        {
            float x = (phase - Mathf.Floor(phase)) * NoiseSamples;
            int i = (int)x;
            if (i >= NoiseSamples) i = NoiseSamples - 1;
            float f = x - i;
            int j = i + 1 == NoiseSamples ? 0 : i + 1;
            return NoiseTable[i] + (NoiseTable[j] - NoiseTable[i]) * f;
        }

        /// <summary>Camera.main is a tagged lookup, so resolve it once per frame for everyone.</summary>
        public static Camera Viewer
        {
            get
            {
                if (viewerFrame == Time.frameCount) return viewer;
                viewerFrame = Time.frameCount;
                if (viewer == null) viewer = Camera.main;
                return viewer;
            }
        }

        /// <summary>How many frames to skip between ticks at this distance from the viewer.</summary>
        public static int Stride(Vector3 position, float nearDistance, float farDistance)
        {
            var cam = Viewer;
            if (cam == null) return 1;

            float sqr = (cam.transform.position - position).sqrMagnitude;
            if (sqr <= nearDistance * nearDistance) return 1;
            if (sqr <= farDistance * farDistance) return 3;
            return 12;
        }
    }
}
