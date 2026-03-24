using UnityEngine;

namespace SeedyRoots.Grid
{
    /// <summary>
    /// Sits on each floor tile.
    /// Uses MaterialPropertyBlock to apply highlight colors without creating material instances.
    /// Supports two independent highlight states: standing (blue) and placement target (yellow).
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class TileHighlighter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private static readonly Color StandingColor  = new Color(0.2f, 0.55f, 1.0f, 1f);
        private static readonly Color PlacementColor = new Color(0.95f, 0.85f, 0.15f, 1f);

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propBlock;
        private Vector2Int registeredCell;
        private Transform snapTransform;

        private bool isStandingHighlighted;
        private bool isPlacementHighlighted;

        /// <summary>The grid cell this tile registered under.</summary>
        public Vector2Int RegisteredCell => registeredCell;

        /// <summary>
        /// World-space position where a GridItem should snap when placed on this tile.
        /// Uses the child "placementSnap" transform when present.
        /// </summary>
        public Vector3 PlacementPosition => snapTransform != null
            ? snapTransform.position
            : transform.position + Vector3.up * 0.5f;

        private void Awake()
        {
            meshRenderer  = GetComponent<MeshRenderer>();
            propBlock     = new MaterialPropertyBlock();
            snapTransform = transform.Find("placementSnap");
        }

        private void Start()
        {
            if (GridMap.Instance == null)
            {
                Debug.LogWarning("[TileHighlighter] GridMap.Instance is null — cannot register tile.", this);
                return;
            }

            registeredCell = GridMap.Instance.WorldToCell(transform.position);
            GridMap.Instance.RegisterTile(registeredCell, this);
        }

        private void OnDestroy()
        {
            if (GridMap.Instance != null)
                GridMap.Instance.UnregisterTile(registeredCell);
        }

        /// <summary>Highlights this tile as the tile the player is currently standing on (blue).</summary>
        public void SetStandingHighlight(bool on)
        {
            if (on == isStandingHighlighted)
                return;

            isStandingHighlighted = on;
            ApplyHighlight();
        }

        /// <summary>Highlights this tile as the active placement target (yellow).</summary>
        public void SetHighlighted(bool on)
        {
            if (on == isPlacementHighlighted)
                return;

            isPlacementHighlighted = on;
            ApplyHighlight();
        }

        // Placement takes priority over standing. Clears the property block when neither is active.
        private void ApplyHighlight()
        {
            meshRenderer.GetPropertyBlock(propBlock);

            if (isPlacementHighlighted)
                propBlock.SetColor(BaseColorId, PlacementColor);
            else if (isStandingHighlighted)
                propBlock.SetColor(BaseColorId, StandingColor);
            else
                propBlock.Clear();

            meshRenderer.SetPropertyBlock(propBlock);
        }
    }
}
