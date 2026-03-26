using System.Collections.Generic;
using SeedyRoots.Items;
using UnityEngine;

namespace SeedyRoots.Grid
{
    /// <summary>
    /// Component on any placeable prop.
    /// Holds the item's current grid cell and carried/placed/stacked state.
    /// Exposes pick-up, follow, place, and stack methods for PlayerInteractionController.
    /// </summary>
    public class GridItem : MonoBehaviour
    {
        public Vector2Int CurrentCell { get; private set; }
        public bool IsPlaced { get; private set; }

        // ── Item Data ─────────────────────────────────────────────────────────

        /// <summary>The ItemData asset describing this item's properties.</summary>
        [SerializeField] private ItemData itemData;
        public ItemData ItemData => itemData;

        // ── Stacking ─────────────────────────────────────────────────────────

        /// <summary>True when this item is sitting on top of a host item's StackMount.</summary>
        public bool IsStacked { get; private set; }

        /// <summary>The item this one is currently stacked on. Null when not stacked.</summary>
        public GridItem StackHost { get; private set; }

        /// <summary>True when another item is currently resting on this item's StackMount.</summary>
        public bool HasStackedItem { get; private set; }

        /// <summary>The item currently stacked on top of this one. Null when empty.</summary>
        public GridItem StackedItem { get; private set; }

        /// <summary>Child transform used as the attachment point for a stacked item. May be null.</summary>
        private Transform stackMount;

        // ── Visual ───────────────────────────────────────────────────────────

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly Color NearbyEmissionColor = new Color(0.4f, 0.8f, 0.2f) * 2f;

        private static readonly List<Renderer> RendererScratch = new List<Renderer>();

        private Collider itemCollider;
        private Renderer itemRenderer;
        private MaterialPropertyBlock propertyBlock;

        private Bounds worldBoundsCache;
        private bool worldBoundsCacheValid;

        private void Awake()
        {
            itemCollider = GetComponent<Collider>();
            itemRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            stackMount = transform.Find("StackMount");

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

            // If the collider is already disabled, OnPickedUp was called before Start ran.
            // This happens when an item is purchased from the store and immediately handed
            // to the player (Instantiate → ReceiveItem → OnPickedUp, then Start runs the
            // next frame at the hand position). Skip grid registration to avoid a phantom
            // entry at the hand's cell that would block placement checks.
            if (itemCollider != null && !itemCollider.enabled)
                return;

            CurrentCell = GridMap.Instance.WorldToCell(transform.position);
            GridMap.Instance.SetItem(CurrentCell, this);
            IsPlaced = true;
        }

        // ── Pick-up / Place ───────────────────────────────────────────────────

        /// <summary>Clears stacking relationships, unregisters from GridMap, and disables the collider while carried.</summary>
        public void OnPickedUp()
        {
            if (IsStacked)
            {
                StackHost.OnStackRemoved();
                StackHost = null;
                IsStacked = false;
            }

            // IsPlaced is never set in OnStackedOn, so stacked items correctly skip ClearCell.
            if (IsPlaced)
                GridMap.Instance.ClearCell(CurrentCell);

            IsPlaced = false;
            worldBoundsCacheValid = false;

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
            worldBoundsCacheValid = false; // position settled; recompute on next request
            GridMap.Instance.SetItem(targetCell, this);
        }

        // ── Stacking ─────────────────────────────────────────────────────────

        /// <summary>True if this item has a StackMount child that can accept a stacked item.</summary>
        public bool HasStackMount => stackMount != null;

        /// <summary>
        /// Attaches this item onto the given host's StackMount child transform.
        /// IsPlaced is intentionally NOT set — so if OnPickedUp is called later,
        /// GridMap.ClearCell is correctly skipped.
        /// Returns true on success, false if the host has no StackMount (caller keeps the item).
        /// </summary>
        public bool OnStackedOn(GridItem host)
        {
            if (host.stackMount == null)
            {
                Debug.LogWarning($"[GridItem] Host '{host.name}' has no StackMount child — add a 'StackMount' child GameObject to its prefab.", this);
                return false;
            }

            StackHost = host;
            IsStacked = true;

            transform.SetParent(host.stackMount, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (itemCollider != null)
                itemCollider.enabled = true;

            host.HasStackedItem = true;
            host.StackedItem = this;
            return true;
        }

        /// <summary>Clears the stacked-item reference when the item on top is picked up.</summary>
        public void OnStackRemoved()
        {
            HasStackedItem = false;
            StackedItem = null;
        }

        // ── Bounds ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the combined world-space bounds of all child renderers.
        /// Cached while the item is placed — invalidated on pick-up and re-place.
        /// Uses a shared static scratch list to avoid per-call allocations.
        /// </summary>
        public Bounds GetWorldBounds()
        {
            if (worldBoundsCacheValid)
                return worldBoundsCache;

            RendererScratch.Clear();
            GetComponentsInChildren(includeInactive: true, RendererScratch);

            if (RendererScratch.Count == 0)
            {
                worldBoundsCache = new Bounds(transform.position, Vector3.zero);
            }
            else
            {
                worldBoundsCache = RendererScratch[0].bounds;
                for (int i = 1; i < RendererScratch.Count; i++)
                    worldBoundsCache.Encapsulate(RendererScratch[i].bounds);
            }

            worldBoundsCacheValid = IsPlaced; // only cache when the transform is stable
            return worldBoundsCache;
        }

        // ── Feedback ──────────────────────────────────────────────────────────

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
