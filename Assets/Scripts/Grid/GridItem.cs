using UnityEngine;

namespace SeedyRoots.Grid
{
    /// <summary>
    /// Component on any placeable prop.
    /// Holds the item's current grid cell and carried/placed state.
    /// Exposes pick-up, follow, and place methods for PlayerInteractionController.
    /// </summary>
    public class GridItem : MonoBehaviour
    {
        public Vector2Int CurrentCell { get; private set; }
        public bool IsPlaced { get; private set; }

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly Color NearbyEmissionColor = new Color(0.4f, 0.8f, 0.2f) * 2f;

        private Collider itemCollider;
        private Renderer itemRenderer;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            itemCollider = GetComponent<Collider>();
            itemRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();

            if (GridMap.Instance == null)
                Debug.LogWarning("[GridItem] GridMap.Instance is null during Awake — ensure GridMap is initialized before GridItem.", this);
        }

        private void Start()
        {
            if (GridMap.Instance == null)
            {
                Debug.LogWarning("[GridItem] GridMap.Instance is null in Start — cannot register item.", this);
                return;
            }

            CurrentCell = GridMap.Instance.WorldToCell(transform.position);
            GridMap.Instance.SetItem(CurrentCell, this);
            IsPlaced = true;
        }

        /// <summary>Unregisters from GridMap and disables the collider while the item is held.</summary>
        public void OnPickedUp()
        {
            if (IsPlaced)
                GridMap.Instance.ClearCell(CurrentCell);

            IsPlaced = false;

            if (itemCollider != null)
                itemCollider.enabled = false;
        }

        /// <summary>Parents this item to the given hand transform and zeroes its local transform each frame.</summary>
        public void FollowHand(Transform handTransform)
        {
            transform.SetParent(handTransform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>Snaps the item to the tile's placement position, re-enables the collider, and registers in GridMap.</summary>
        public void OnPlaced(Vector2Int targetCell, Vector3 snapPosition)
        {
            transform.SetParent(null);
            CurrentCell = targetCell;
            transform.position = snapPosition;
            transform.rotation = Quaternion.identity;

            if (itemCollider != null)
                itemCollider.enabled = true;

            IsPlaced = true;
            GridMap.Instance.SetItem(targetCell, this);
        }

        /// <summary>
        /// Drives the green emission glow on the item to signal it is within pickup range.
        /// Enables the _EMISSION keyword on the material so URP Lit evaluates it,
        /// then uses MaterialPropertyBlock to avoid per-call material instances.
        /// </summary>
        public void SetNearbyFeedback(bool isNearby)
        {
            if (itemRenderer == null)
                return;

            // Toggle the _EMISSION keyword on the shared material so the shader reads the colour.
            if (isNearby)
                itemRenderer.material.EnableKeyword("_EMISSION");
            else
                itemRenderer.material.DisableKeyword("_EMISSION");

            itemRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, isNearby ? NearbyEmissionColor : Color.black);
            itemRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
