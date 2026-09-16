using MeowStudio.VFX;
using UnityEngine;

namespace MeowStudio.VR
{
    /// <summary>
    /// Spawns the shared flame VFX on the wick anchor and resizes it, since the prefab
    /// is authored for a candle. All burning behaviour lives in <see cref="FlameBehaviour"/>.
    /// </summary>
    public class LighterFlame: MonoBehaviour
    {
        [SerializeField] private GameObject flamePrefab;
        [SerializeField] private Transform anchor;

        [Header("Candle prefab overrides")]
        [SerializeField] private float flameScale = 0.45f;
        [SerializeField] private float lightRange = 0.6f;
        [SerializeField] private float lightIntensity = 0.35f;

        private FlameBehaviour flame;

        public bool isBurning => flame != null && flame.isBurning;
        public FlameBehaviour behaviour => flame;

        private void Awake()
        {
            if (flamePrefab == null || anchor == null)
            {
                Debug.LogError($"{nameof(LighterFlame)} on '{name}' needs both a flame prefab and an anchor.", this);
                return;
            }

            var instance = Instantiate(flamePrefab, anchor);
            instance.transform.localPosition = Vector3.zero;
            // Align the flame with the lighter's own up axis, not the anchor's: the model
            // comes from Blender, so the anchor's local Y points sideways in world space.
            instance.transform.rotation = transform.rotation;
            instance.transform.localScale = Vector3.one * flameScale;

            var particles = instance.GetComponentInChildren<ParticleSystem>(true);
            if (particles != null)
            {
                // Shape scaling would leave the particles themselves candle-sized.
                var main = particles.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }

            var light = instance.GetComponentInChildren<Light>(true);
            if (light != null)
            {
                if (lightRange > 0f) light.range = lightRange;
                if (lightIntensity > 0f) light.intensity = lightIntensity;
            }

            flame = instance.GetComponent<FlameBehaviour>();
            if (flame == null)
            {
                Debug.LogError($"Flame prefab '{flamePrefab.name}' has no {nameof(FlameBehaviour)}.", this);
                return;
            }

            // The lighter drives the flame from the trigger, so it must not self-ignite.
            flame.SetLightOnStart(false);

            // The light values above were changed after the instance woke up.
            flame.CaptureBaseValues();
            flame.Extinguish();
            instance.SetActive(false);
        }

        public void Show()
        {
            if (flame != null) flame.Light();
        }

        public void Hide()
        {
            if (flame != null) flame.Extinguish();
        }

        public void HideInstant()
        {
            if (flame == null) return;
            flame.Extinguish();
            flame.gameObject.SetActive(false);
        }
    }
}
