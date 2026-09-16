using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MeowStudio.VR.Interaction
{
    /// <summary>
    /// Adds a cheap inverted-hull outline to any XR interactable while it can be grabbed:
    /// visible from the first hover - ray, direct hand, or gaze, whichever fires it - until
    /// hover ends, and hidden again once the object is actually picked up (it doesn't need
    /// the "you can grab this" cue anymore once you're holding it).
    ///
    /// Works by appending one shared outline material to each renderer's material list.
    /// Toggling swaps a cached array reference - no allocation, no per-object material
    /// instance - so the only runtime cost is a handful of extra unlit, textureless draw
    /// calls while something is actually highlighted, which is negligible even on Quest.
    /// </summary>
    [RequireComponent(typeof(XRBaseInteractable))]
    public class InteractableOutline: MonoBehaviour
    {
        [Tooltip("Renderers to outline. Auto-collected from children when left empty.")]
        [SerializeField] private Renderer[] renderers;

        [SerializeField] private Color outlineColor = new Color(0.35f, 0.85f, 1f, 1f);
        [Tooltip("World-space thickness in meters.")]
        [SerializeField] private float outlineWidth = 0.0025f;
        [Tooltip("Hide the outline again once the object is actually grabbed.")]
        [SerializeField] private bool hideWhileSelected = true;

        private static Material sharedOutlineMaterial;
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

        private XRBaseInteractable interactable;
        private Material[][] plainMaterials;
        private Material[][] outlinedMaterials;
        private bool isOn;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);

            if (sharedOutlineMaterial == null)
            {
                var shader = Shader.Find("MeowStudio/FX/InteractableOutline");
                if (shader != null) sharedOutlineMaterial = new Material(shader) { name = "InteractableOutline (shared)" };
                else Debug.LogError($"{nameof(InteractableOutline)}: shader 'MeowStudio/FX/InteractableOutline' not found.", this);
            }

            plainMaterials = new Material[renderers.Length][];
            outlinedMaterials = new Material[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;

                var plain = r.sharedMaterials;
                plainMaterials[i] = plain;
                if (sharedOutlineMaterial == null) continue;

                var withOutline = new Material[plain.Length + 1];
                System.Array.Copy(plain, withOutline, plain.Length);
                withOutline[plain.Length] = sharedOutlineMaterial;
                outlinedMaterials[i] = withOutline;

                // Setting the property block here would be a no-op: Unity only accepts a
                // per-slot block for a slot the renderer's material array already has, and
                // reassigning sharedMaterials later (even the same array, e.g. on an off/on
                // toggle) drops whatever was set before. So the block is (re)applied in
                // SetOutline every time the outline turns on, once the slot actually exists.
            }
        }

        private void OnEnable()
        {
            interactable.firstHoverEntered.AddListener(OnFirstHoverEntered);
            interactable.lastHoverExited.AddListener(OnLastHoverExited);
            if (hideWhileSelected)
            {
                interactable.selectEntered.AddListener(OnSelectEntered);
                interactable.selectExited.AddListener(OnSelectExited);
            }
        }

        private void OnDisable()
        {
            interactable.firstHoverEntered.RemoveListener(OnFirstHoverEntered);
            interactable.lastHoverExited.RemoveListener(OnLastHoverExited);
            if (hideWhileSelected)
            {
                interactable.selectEntered.RemoveListener(OnSelectEntered);
                interactable.selectExited.RemoveListener(OnSelectExited);
            }
            SetOutline(false);
        }

        private void OnFirstHoverEntered(HoverEnterEventArgs args)
        {
            if (hideWhileSelected && interactable.isSelected) return;
            SetOutline(true);
        }

        private void OnLastHoverExited(HoverExitEventArgs args) => SetOutline(false);

        private void OnSelectEntered(SelectEnterEventArgs args) => SetOutline(false);

        private void OnSelectExited(SelectExitEventArgs args)
        {
            // Still hovered by another interactor (e.g. the other hand) after this one let go.
            if (interactable.isHovered) SetOutline(true);
        }

        [Button("Toggle Outline")]
        private void SetOutline(bool on)
        {
            if (isOn == on || sharedOutlineMaterial == null) return;
            isOn = on;

            var block = on ? new MaterialPropertyBlock() : null;
            if (on)
            {
                block.SetColor(OutlineColorId, outlineColor);
                block.SetFloat(OutlineWidthId, outlineWidth);
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                r.sharedMaterials = on ? outlinedMaterials[i] : plainMaterials[i];
                if (on) r.SetPropertyBlock(block, plainMaterials[i].Length);
            }
        }
    }
}
