using UnityEngine;

namespace MeowStudio.VFX
{
    /// <summary>
    /// Drives one glow sphere: its scale and the shader's _Intensity.
    ///
    /// The intensity goes through a MaterialPropertyBlock rather than renderer.material,
    /// so nothing else using FX_CandleGlow is affected and no material instance is
    /// created per flame (one instance per candle would be a real leak). The cost is
    /// that this renderer drops out of the SRP Batcher, which is a non-issue for a
    /// handful of additive transparent spheres.
    /// </summary>
    public sealed class FlameGlowDriver
    {
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        private Renderer target;
        private Transform targetTransform;
        private MaterialPropertyBlock block;

        private Vector3 baseScale = Vector3.one;
        private float baseIntensity = 1f;
        private bool drivesIntensity;

        public bool isBound => target != null || targetTransform != null;

        public void Bind(Renderer renderer, Transform transform)
        {
            target = renderer;
            targetTransform = transform != null ? transform : (renderer != null ? renderer.transform : null);

            var material = target != null ? target.sharedMaterial : null;
            drivesIntensity = material != null && material.HasProperty(IntensityId);
            if (drivesIntensity && block == null) block = new MaterialPropertyBlock();

            CaptureBase();
        }

        /// <summary>Re-reads the authored scale and intensity. Not while they are being driven.</summary>
        public void CaptureBase()
        {
            if (targetTransform != null) baseScale = targetTransform.localScale;
            if (drivesIntensity) baseIntensity = target.sharedMaterial.GetFloat(IntensityId);
        }

        public void SetVisible(bool value)
        {
            if (target != null) target.enabled = value;
        }

        public void Apply(float scaleFactor, float intensityFactor)
        {
            if (targetTransform != null) targetTransform.localScale = baseScale * scaleFactor;
            if (!drivesIntensity) return;

            // GetPropertyBlock fills the existing block instead of allocating, and keeps
            // any override somebody else may have set on this renderer.
            target.GetPropertyBlock(block);
            block.SetFloat(IntensityId, baseIntensity * intensityFactor);
            target.SetPropertyBlock(block);
        }

        /// <summary>Back to exactly how the prefab was authored.</summary>
        public void Restore()
        {
            if (targetTransform != null) targetTransform.localScale = baseScale;
            if (!drivesIntensity) return;

            target.GetPropertyBlock(block);
            block.SetFloat(IntensityId, baseIntensity);
            target.SetPropertyBlock(block);
        }
    }
}
