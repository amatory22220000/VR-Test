using System.Collections.Generic;
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
    ///
    /// Handles both MeshRenderer and SkinnedMeshRenderer, and both faceted (flat-shaded,
    /// hard-edge) and smooth-shaded low-poly meshes correctly - see EnsureSmoothNormals.
    /// Not yet handled: a renderer with more than one submesh only gets an outline around
    /// its last submesh (a Debug.LogWarning fires when this happens; see GetSubMeshCount).
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

        // Meshes whose smooth-normal channel has already been baked (see
        // EnsureSmoothNormals). Keyed by the shared Mesh instance, so this runs once
        // total no matter how many renderers or interactable instances use it.
        private static readonly HashSet<Mesh> smoothNormalsBaked = new HashSet<Mesh>();

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

                EnsureSmoothNormals(GetSharedMesh(r));

                int subMeshCount = GetSubMeshCount(r);
                if (subMeshCount > 1)
                {
                    // Unity only draws extra material-array entries against the LAST
                    // submesh, so simply appending one material here would trace an
                    // outline around a single submesh, not the whole object. Handling
                    // this properly needs a second renderer covering every submesh, not
                    // yet built - flag it loudly instead of shipping a silently partial
                    // outline on the next multi-material asset that gets this component.
                    Debug.LogWarning($"{nameof(InteractableOutline)} on '{name}': renderer '{r.name}' has " +
                        $"{subMeshCount} submeshes; only the last one will get an outline. " +
                        "Multi-submesh support isn't implemented yet.", this);
                }

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

        /// <summary>Mesh a renderer actually draws, whichever kind of renderer it is.</summary>
        private static Mesh GetSharedMesh(Renderer r)
        {
            if (r is SkinnedMeshRenderer skinned) return skinned.sharedMesh;
            var filter = r.GetComponent<MeshFilter>();
            return filter != null ? filter.sharedMesh : null;
        }

        private static int GetSubMeshCount(Renderer r)
        {
            var mesh = GetSharedMesh(r);
            return mesh != null ? mesh.subMeshCount : 1;
        }

        /// <summary>
        /// Bakes a per-position-averaged normal into TEXCOORD2 for the outline shader to
        /// extrude along, instead of the mesh's real NORMAL.
        ///
        /// Our props are flat-shaded/faceted low-poly: every hard edge has duplicate
        /// vertices at the same position with different normals. Extruding along the real
        /// normal pushes the two faces at that edge apart in different directions, so the
        /// inverted-hull shell tears open there - it only stays sealed where neighbouring
        /// faces happen to agree, which reads as "some edges outlined, others not" rather
        /// than a full silhouette. Averaging by position first gives every vertex sharing
        /// a location the same extrusion direction, so the shell stays continuous at every
        /// edge regardless of shading.
        ///
        /// Runs once per unique Mesh (cached), not per instance or per frame: negligible
        /// even for many outlined objects, and it never touches the source asset - this
        /// mutates the runtime-loaded Mesh object, not the file on disk.
        /// </summary>
        private static void EnsureSmoothNormals(Mesh mesh)
        {
            if (mesh == null || !smoothNormalsBaked.Add(mesh)) return;

            var positions = mesh.vertices;
            var normals = mesh.normals;
            if (positions.Length == 0 || normals.Length != positions.Length) return;

            // Group vertices by position (hard-edge duplicates share a location but not
            // an index), rounding to fold together anything that's the same point up to
            // floating-point noise.
            var groups = new Dictionary<Vector3Int, List<int>>();
            const float unitsPerMeter = 100000f; // 10-micron buckets: plenty for prop-scale meshes
            for (int i = 0; i < positions.Length; i++)
            {
                var p = positions[i];
                var key = new Vector3Int(
                    Mathf.RoundToInt(p.x * unitsPerMeter),
                    Mathf.RoundToInt(p.y * unitsPerMeter),
                    Mathf.RoundToInt(p.z * unitsPerMeter));
                if (!groups.TryGetValue(key, out var list)) groups[key] = list = new List<int>();
                list.Add(i);
            }

            var smoothed = new Vector3[positions.Length];
            foreach (var group in groups.Values)
            {
                var average = Vector3.zero;
                foreach (var i in group) average += normals[i];
                average = average.sqrMagnitude > 1e-10f ? average.normalized : Vector3.up;
                foreach (var i in group) smoothed[i] = average;
            }

            mesh.SetUVs(2, new List<Vector3>(smoothed)); // TEXCOORD2: unused by these props (no lightmap/detail UVs)
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
