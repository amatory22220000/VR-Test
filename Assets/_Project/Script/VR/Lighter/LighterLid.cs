using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MeowStudio.VR
{
    /// <summary>
    /// Opens and closes the lighter lid around its hinge.
    /// The pivot comes from the model and already sits on the hinge line,
    /// so the whole animation is a rotation around the lid's local Y.
    /// </summary>
    public class LighterLid: MonoBehaviour
    {
        [SerializeField] private Transform lid;
        [SerializeField] private float openAngle = -120f;
        [SerializeField] private float openDuration = 0.18f;
        [SerializeField] private float closeDuration = 0.14f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InQuad;

        private Tween tween;
        private float angle;

        public bool isOpen { get; private set; }

        private void Awake()
        {
            // Never fall back to this.transform: the component sits on the prefab root,
            // and a missing reference would zero the whole lighter's rotation instead.
            if (lid == null)
            {
                Debug.LogError($"{nameof(LighterLid)} on '{name}' has no lid transform assigned.", this);
                enabled = false;
                return;
            }

            SetClosedInstant();
        }

        public void Open()
        {
            if (!enabled || isOpen) return;
            isOpen = true;
            PlayTo(openAngle, openDuration, openEase);
        }
        public void Close()
        {
            if (!enabled || !isOpen) return;
            isOpen = false;
            PlayTo(0f, closeDuration, closeEase);
        }

        public void SetClosedInstant()
        {
            if (lid == null) return;
            KillTween();
            isOpen = false;
            Apply(0f);
        }

        // Driving a float instead of DOLocalRotate on purpose: DOTween's rotate modes
        // read transform.localEulerAngles, which wraps into 0..360 and picks a shortest
        // path that is ambiguous when the tween is interrupted mid-swing. A plain float
        // keeps the hinge single-axis and fully deterministic.
        private void PlayTo(float target, float duration, Ease ease)
        {
            KillTween();
            tween = DOTween.To(() => angle, Apply, target, duration).SetEase(ease);
        }

        private void Apply(float value)
        {
            angle = value;
            lid.localRotation = Quaternion.Euler(0f, angle, 0f);
        }

        private void KillTween()
        {
            if (tween == null) return;
            tween.Kill();
            tween = null;
        }

        private void OnDestroy()
        {
            KillTween();
        }
    }
}
