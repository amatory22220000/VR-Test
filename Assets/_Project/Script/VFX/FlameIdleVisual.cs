using UnityEngine;

namespace MeowStudio.VFX
{
    /// <summary>
    /// The cheap flame for candles lit by baked lighting: it only breathes the glow and
    /// the particle size so the fire is not frozen. No realtime Light is touched, because
    /// on a baked candle that light is in the lightmap already. No tweens, no upright
    /// correction, no blow-out — those belong to <see cref="FlameBehaviour"/>.
    /// </summary>
    public class FlameIdleVisual: MonoBehaviour
    {
        [Header("Parts (auto-found when empty)")]
        [SerializeField] private Transform glow;
        [SerializeField] private Renderer glowRenderer;
        [SerializeField] private ParticleSystem particles;

        [Header("Glow flicker")]
        [SerializeField] private bool flickerGlow = true;
        [SerializeField, Range(0f, 1f)] private float glowFlicker = 0.1f;
        [SerializeField] private float glowFlickerRate = 3.1f;
        [Tooltip("Scale wobbles a little less than brightness, which reads calmer.")]
        [SerializeField, Range(0f, 1f)] private float glowScaleShare = 0.5f;

        [Header("Particle flicker")]
        [Tooltip("Varies the size of newly spawned particles only, so it is nearly free.")]
        [SerializeField] private bool flickerParticleSize = true;
        [SerializeField, Range(0f, 1f)] private float particleSizeFlicker = 0.12f;
        [SerializeField] private float particleSizeFlickerRate = 1.6f;

        [Header("Distance LOD")]
        [Tooltip("Full rate closer than this, every 3rd frame up to the far range, every 12th beyond it.")]
        [SerializeField] private float nearDistance = 3f;
        [SerializeField] private float farDistance = 9f;

        private readonly FlameGlowDriver glowDriver = new FlameGlowDriver();
        private Transform cachedTransform;
        private float baseParticleSize = 1f;
        private bool drivesParticleSize;

        private float glowPhase, particlePhase;
        private float tickTime;
        private int frameCountdown = 1;
        private int stride = 1;

        private void Awake()
        {
            cachedTransform = transform;

            if (glowRenderer == null) glowRenderer = GetComponentInChildren<MeshRenderer>(true);
            if (glow == null && glowRenderer != null) glow = glowRenderer.transform;
            if (particles == null) particles = GetComponentInChildren<ParticleSystem>(true);

            glowDriver.Bind(glowRenderer, glow);

            drivesParticleSize = flickerParticleSize && particles != null;
            if (drivesParticleSize) baseParticleSize = particles.main.startSizeMultiplier;

            glowPhase = Random.value;
            particlePhase = Random.value;
        }

        private void OnEnable()
        {
            frameCountdown = 1;
        }

        private void OnDisable()
        {
            // Leave the prefab exactly as authored, so nothing looks half-dimmed in the editor.
            glowDriver.Restore();
            if (drivesParticleSize)
            {
                var main = particles.main;
                main.startSizeMultiplier = baseParticleSize;
            }
        }

        private void Update()
        {
            tickTime += Time.deltaTime;
            if (--frameCountdown > 0) return;

            float dt = tickTime;
            tickTime = 0f;
            if (dt <= 0f) { frameCountdown = 1; return; }

            Tick(dt);
            frameCountdown = stride;
        }

        private void Tick(float dt)
        {
            if (flickerGlow && glowDriver.isBound)
            {
                glowPhase += glowFlickerRate * dt;
                float wobble = FlameFx.Sample(glowPhase) * glowFlicker;
                glowDriver.Apply(1f + wobble * glowScaleShare, 1f + wobble);
            }

            if (drivesParticleSize)
            {
                particlePhase += particleSizeFlickerRate * dt;
                var main = particles.main;
                main.startSizeMultiplier = baseParticleSize * (1f + FlameFx.Sample(particlePhase) * particleSizeFlicker);
            }

            stride = FlameFx.Stride(cachedTransform.position, nearDistance, farDistance);
        }
    }
}
