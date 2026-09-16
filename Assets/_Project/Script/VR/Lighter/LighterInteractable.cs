using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MeowStudio.VR
{
    /// <summary>
    /// Drives the lighter from XR input: holding the controller trigger while the
    /// lighter is grabbed flips the lid open and lights the flame, releasing it
    /// puts the flame out and snaps the lid shut.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class LighterInteractable: MonoBehaviour
    {
        [SerializeField] private LighterLid lid;
        [SerializeField] private LighterFlame flame;

        [Tooltip("Lets the lid clear the wick before the flame appears.")]
        [SerializeField] private float igniteDelay = 0.1f;

        private XRGrabInteractable interactable;
        private Tween igniteTween;

        public bool isLit { get; private set; }

        private void Awake()
        {
            interactable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            interactable.activated.AddListener(OnActivated);
            interactable.deactivated.AddListener(OnDeactivated);
            interactable.selectExited.AddListener(OnSelectExited);
        }

        private void OnDisable()
        {
            interactable.activated.RemoveListener(OnActivated);
            interactable.deactivated.RemoveListener(OnDeactivated);
            interactable.selectExited.RemoveListener(OnSelectExited);
            Extinguish();
        }

        [Button("Ignite")]
        public void Ignite()
        {
            if (isLit) return;
            isLit = true;

            if (lid != null) lid.Open();

            KillIgniteTween();
            if (igniteDelay > 0f) igniteTween = DOVirtual.DelayedCall(igniteDelay, ShowFlame);
            else ShowFlame();
        }

        [Button("Extinguish")]
        public void Extinguish()
        {
            if (!isLit) return;
            isLit = false;

            KillIgniteTween();
            if (flame != null) flame.Hide();
            if (lid != null) lid.Close();
        }

        private void OnActivated(ActivateEventArgs args) => Ignite();

        private void OnDeactivated(DeactivateEventArgs args) => Extinguish();

        // Dropping or throwing the lighter with the trigger still held must not
        // leave it burning in mid-air.
        private void OnSelectExited(SelectExitEventArgs args) => Extinguish();

        private void ShowFlame()
        {
            igniteTween = null;
            if (flame != null) flame.Show();
        }

        private void KillIgniteTween()
        {
            if (igniteTween == null) return;
            igniteTween.Kill();
            igniteTween = null;
        }

        private void OnDestroy()
        {
            KillIgniteTween();
        }
    }
}
