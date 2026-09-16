using System;
using DG.Tweening;
using UnityEngine;

namespace MeowStudio.VFX
{
    /// <summary>
    /// A flame that lights and dies on a tween, keeps pointing up while its holder
    /// rotates, leans against the holder's acceleration, and blows out when the holder
    /// is turned over. Lives on the flame prefab itself so candles and the lighter share it.
    ///
    /// For a candle lit by baked lighting use <see cref="FlameIdleVisual"/> instead:
    /// this one drives a realtime Light and costs more than such a candle needs.
    /// </summary>
    public class FlameBehaviour: MonoBehaviour
    {
        [Header("Parts (auto-found when empty)")]
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private Transform glow;
        [SerializeField] private Renderer glowRenderer;
        [SerializeField] private new Light light;

        [Tooltip("A flame placed in a scene lights itself. Spawners that drive it explicitly turn this off.")]
        [SerializeField] private bool lightOnStart = true;

        [Header("Ignition")]
        [SerializeField] private float igniteDuration = 0.28f;
        [SerializeField] private Ease igniteEase = Ease.OutQuad;
        [Tooltip("Blowing out is meant to be much faster than lighting up.")]
        [SerializeField] private float extinguishDuration = 0.07f;
        [SerializeField] private Ease extinguishEase = Ease.InQuad;
        [Tooltip("Glow scale at zero burn, as a fraction of the authored scale. The halo grows from here.")]
        [SerializeField, Range(0f, 1f)] private float glowStartScale = 0.3f;
        [Tooltip("Particles seeded the instant the flame lights, so the trigger is not followed by a dead beat.")]
        [SerializeField, Range(0, 10)] private int igniteBurst = 2;

        [Header("Flicker while burning")]
        [SerializeField] private bool flicker = true;
        [SerializeField, Range(0f, 1f)] private float lightFlicker = 0.16f;
        [SerializeField] private float lightFlickerRate = 4.5f;
        [SerializeField, Range(0f, 1f)] private float glowFlicker = 0.08f;
        [SerializeField] private float glowFlickerRate = 3.1f;

        [Header("Upright")]
        [SerializeField] private bool keepUpright = true;
        [Tooltip("Critically damped, so the flame settles without swinging like a pendulum.")]
        [SerializeField] private float uprightSmoothTime = 0.15f;
        [Tooltip("Degrees of lean per 1 m/s^2 the holder accelerates. This is what stops it reading as a gimbal.")]
        [SerializeField] private float leanPerAcceleration = 1.8f;
        [SerializeField] private float maxLeanAngle = 35f;

        [Header("Idle motion")]
        [SerializeField] private float swayAngle = 2.5f;
        [SerializeField] private float swayRate = 0.35f;
        [SerializeField] private float trembleAngle = 1.4f;
        [SerializeField] private float trembleRate = 5f;
        [Tooltip("Extra tremble at speed: degrees added per 1 m/s of holder movement.")]
        [SerializeField] private float tremblePerSpeed = 0.6f;

        [Header("Blow out")]
        [SerializeField] private bool blowOutOnFlip = true;
        [Tooltip("Holder tilt that puts the flame out.")]
        [SerializeField] private float blowOutAngle = 115f;
        [Tooltip("Must come back below this before it can relight (hysteresis).")]
        [SerializeField] private float relightAngle = 75f;
        [Tooltip("How long the tilt must hold, so a quick flick does not kill it.")]
        [SerializeField] private float flipDwell = 0.3f;
        [SerializeField] private bool blowOutOnSpeed;
        [SerializeField] private float blowOutSpeed = 3.5f;
        [SerializeField] private float speedDwell = 0.15f;
        [Tooltip("Candles want this off: once knocked out they stay out until relit.")]
        [SerializeField] private bool relightWhenUpright = true;

        [Header("Distance LOD")]
        [Tooltip("Full rate closer than this, every 3rd frame up to the far range, every 12th beyond it.")]
        [SerializeField] private float nearDistance = 3f;
        [SerializeField] private float farDistance = 9f;

        public event Action OnBlownOut;

        public bool isBurning { get; private set; }
        public bool wantsToBurn { get; private set; }

        /// <summary>0 when out, 1 at full burn. Driven by the ignite/extinguish tween.</summary>
        public float burnFactor { get; private set; }

        private readonly FlameGlowDriver glowDriver = new FlameGlowDriver();
        private Tween burnTween;

        private Transform cachedTransform;
        private Transform holder;

        private Vector3 currentUp = Vector3.up;
        private Vector3 upDampVelocity;
        private Vector3 lastPosition;
        private Vector3 lastVelocity;
        private bool hasPreviousSample;

        private float flipTimer;
        private float speedTimer;
        private bool blownOut;

        private float baseLightIntensity = 1f;
        private Vector3 restLocalUp = Vector3.up;

        private float swayPhase, tremblePhase, lightPhase, glowPhase;
        private float tickTime;
        private int frameCountdown = 1;
        private int stride = 1;

        private void Awake()
        {
            cachedTransform = transform;
            holder = cachedTransform.parent;

            if (particles == null) particles = GetComponentInChildren<ParticleSystem>(true);
            if (light == null) light = GetComponentInChildren<Light>(true);
            if (glowRenderer == null) glowRenderer = GetComponentInChildren<MeshRenderer>(true);
            if (glow == null && glowRenderer != null) glow = glowRenderer.transform;
            glowDriver.Bind(glowRenderer, glow);

            swayPhase = UnityEngine.Random.value;
            tremblePhase = UnityEngine.Random.value;
            lightPhase = UnityEngine.Random.value;
            glowPhase = UnityEngine.Random.value;

            CaptureBaseValues();
        }

        /// <summary>
        /// Re-reads the authored glow scale, glow intensity and light intensity. Call it
        /// after tweaking those on a spawned instance (the lighter shrinks the candle-sized
        /// prefab). Harmless to call while out, wrong to call mid-burn.
        /// </summary>
        public void CaptureBaseValues()
        {
            if (burnTween == null && !isBurning)
            {
                glowDriver.CaptureBase();
                if (light != null) baseLightIntensity = light.intensity;

                // Which way is "up" for the holder cannot be assumed to be its Y: a Blender
                // FBX arrives with a rotated root, so an anchor inside it has Y pointing
                // sideways. Read it from the pose the flame was placed in instead.
                restLocalUp = (cachedTransform.localRotation * Vector3.up).normalized;
            }
        }

        /// <summary>Call before Start on a spawned instance whose owner drives it explicitly.</summary>
        public void SetLightOnStart(bool value) => lightOnStart = value;

        public void Light()
        {
            wantsToBurn = true;
            blownOut = false;
            flipTimer = 0f;
            speedTimer = 0f;
            StartBurning();
        }

        public void Extinguish()
        {
            wantsToBurn = false;
            blownOut = false;

            if (isBurning) StopBurning();
            else if (burnTween == null) GoDormant();
        }

        private void StartBurning()
        {
            if (isBurning) return;
            isBurning = true;

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            glowDriver.SetVisible(true);
            if (light != null) light.enabled = true;

            // Start from empty so the flame builds instead of popping in fully formed, then
            // seed a couple of particles by hand. Emission alone would leave a dead beat
            // before the first one appears, and a burst authored in the prefab is no good
            // here: on a looping system it would fire again on every loop.
            if (particles != null)
            {
                particles.Clear(true);
                particles.Play(true);
                if (igniteBurst > 0) particles.Emit(igniteBurst);
            }

            currentUp = Vector3.up;
            upDampVelocity = Vector3.zero;
            hasPreviousSample = false;

            ApplyBurnVisuals();
            PlayBurnTween(1f, igniteDuration, igniteEase, null);
        }

        private void StopBurning()
        {
            if (!isBurning) return;
            isBurning = false;

            if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            PlayBurnTween(0f, extinguishDuration, extinguishEase, GoDormant);
        }

        /// <summary>
        /// One cached tween drives the whole ignition. Starting a new one kills the old,
        /// so a blow-out that lands halfway through lighting up just takes over from there
        /// instead of popping, and the duration scales with how far it still has to travel.
        /// </summary>
        private void PlayBurnTween(float target, float duration, Ease ease, TweenCallback onDone)
        {
            KillBurnTween();

            float distance = Mathf.Abs(target - burnFactor);
            if (duration <= 0f || distance <= 0.001f)
            {
                burnFactor = target;
                ApplyBurnVisuals();
                onDone?.Invoke();
                return;
            }

            burnTween = DOTween.To(() => burnFactor, SetBurnFactor, target, duration * distance)
                .SetEase(ease)
                .OnComplete(() =>
                {
                    burnTween = null;
                    onDone?.Invoke();
                });
        }

        private void SetBurnFactor(float value)
        {
            burnFactor = value;
            ApplyBurnVisuals();
        }

        private void KillBurnTween()
        {
            if (burnTween == null) return;
            burnTween.Kill();
            burnTween = null;
        }

        private void ApplyBurnVisuals()
        {
            float lightWobble = 1f;
            float glowWobble = 1f;
            if (flicker && burnFactor > 0f)
            {
                lightWobble = 1f + FlameFx.Sample(lightPhase) * lightFlicker;
                glowWobble = 1f + FlameFx.Sample(glowPhase) * glowFlicker;
            }

            if (light != null) light.intensity = baseLightIntensity * burnFactor * lightWobble;

            glowDriver.Apply(Mathf.Lerp(glowStartScale, 1f, burnFactor) * glowWobble,
                             burnFactor * glowWobble);
        }

        private void Start()
        {
            if (lightOnStart) Light();
        }

        private void OnEnable()
        {
            hasPreviousSample = false;
            frameCountdown = 1;
        }

        private void OnDestroy()
        {
            KillBurnTween();
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
            Vector3 position = cachedTransform.position;

            Vector3 velocity = Vector3.zero;
            Vector3 acceleration = Vector3.zero;
            if (hasPreviousSample)
            {
                velocity = (position - lastPosition) / dt;
                acceleration = (velocity - lastVelocity) / dt;
            }
            lastPosition = position;
            lastVelocity = velocity;
            hasPreviousSample = true;

            if (isBurning)
            {
                if (flicker)
                {
                    lightPhase += lightFlickerRate * dt;
                    glowPhase += glowFlickerRate * dt;
                    // The tween also calls this every frame while it runs; here it keeps the
                    // flicker alive for the rest of the burn.
                    if (burnTween == null) ApplyBurnVisuals();
                }

                if (keepUpright) UpdateOrientation(dt, velocity, acceleration);
                UpdateBlowOut(dt, velocity);
            }
            else if (blownOut && wantsToBurn && relightWhenUpright)
            {
                UpdateRelight();
            }

            stride = burnTween != null ? 1 : FlameFx.Stride(position, nearDistance, farDistance);
        }

        /// <summary>World direction that counts as "up" for the holder in its rest pose.</summary>
        private Vector3 HolderUp()
        {
            return holder != null ? holder.TransformDirection(restLocalUp) : Vector3.up;
        }

        private void UpdateOrientation(float dt, Vector3 velocity, Vector3 acceleration)
        {
            // A real flame bends away from the acceleration, not towards the tilt.
            Vector3 lean = -acceleration * (leanPerAcceleration * Mathf.Deg2Rad);
            lean.y = 0f;
            float maxLean = Mathf.Tan(maxLeanAngle * Mathf.Deg2Rad);
            if (lean.sqrMagnitude > maxLean * maxLean) lean = lean.normalized * maxLean;

            swayPhase += swayRate * dt;
            tremblePhase += trembleRate * dt;

            float wobble = (trembleAngle + velocity.magnitude * tremblePerSpeed) * Mathf.Deg2Rad;
            float sway = swayAngle * Mathf.Deg2Rad;

            Vector3 target = Vector3.up + lean;
            target.x += FlameFx.Sample(swayPhase) * sway + FlameFx.Sample(tremblePhase) * wobble;
            target.z += FlameFx.Sample(swayPhase + 0.37f) * sway + FlameFx.Sample(tremblePhase + 0.61f) * wobble;

            currentUp = Vector3.SmoothDamp(currentUp, target.normalized, ref upDampVelocity, uprightSmoothTime, float.MaxValue, dt);

            float sqr = currentUp.sqrMagnitude;
            if (sqr < 1e-6f) { currentUp = Vector3.up; return; }
            currentUp /= Mathf.Sqrt(sqr);

            cachedTransform.rotation = Quaternion.FromToRotation(Vector3.up, currentUp);
        }

        private void UpdateBlowOut(float dt, Vector3 velocity)
        {
            if (blowOutOnFlip && holder != null)
            {
                if (Vector3.Angle(HolderUp(), Vector3.up) >= blowOutAngle)
                {
                    flipTimer += dt;
                    if (flipTimer >= flipDwell) { BlowOut(); return; }
                }
                else flipTimer = 0f;
            }

            if (blowOutOnSpeed)
            {
                if (velocity.sqrMagnitude >= blowOutSpeed * blowOutSpeed)
                {
                    speedTimer += dt;
                    if (speedTimer >= speedDwell) { BlowOut(); return; }
                }
                else speedTimer = 0f;
            }
        }

        private void UpdateRelight()
        {
            if (holder == null) return;
            if (Vector3.Angle(HolderUp(), Vector3.up) > relightAngle) return;

            blownOut = false;
            flipTimer = 0f;
            speedTimer = 0f;
            StartBurning();
        }

        private void BlowOut()
        {
            blownOut = true;
            flipTimer = 0f;
            speedTimer = 0f;
            StopBurning();
            OnBlownOut?.Invoke();
        }

        /// <summary>
        /// Everything off and reset to the authored values. The object itself stays alive
        /// only while a relight is still possible, since Update has to keep running for it.
        /// </summary>
        private void GoDormant()
        {
            burnFactor = 0f;
            glowDriver.Restore();
            glowDriver.SetVisible(false);
            if (light != null)
            {
                light.intensity = baseLightIntensity;
                light.enabled = false;
            }
            if (particles != null) particles.Clear(true);

            if (!(blownOut && wantsToBurn && relightWhenUpright)) gameObject.SetActive(false);
        }
    }
}
