using SeedyRoots.Core;
using SeedyRoots.Grid;
using SeedyRoots.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SeedyRoots.Player
{
    /// <summary>
    /// Drives the full pick-up / place loop on the Player GameObject.
    /// The forward tile tracked by PlayerForwardTileTracker is the single interaction
    /// target for both pickup and placement: if that tile has a GridItem registered,
    /// the player can pick it up; if it is free, the player can place a held item there.
    /// Receives the Interact input action via the PlayerInput SendMessages behaviour.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerForwardTileTracker))]
    public class PlayerInteractionController : MonoBehaviour
    {
        [Tooltip("The empty Transform that defines where held items are parented.")]
        [SerializeField] private Transform handTransform;

        private PlayerForwardTileTracker forwardTileTracker;

        private GridItem nearestItem;
        private GridItem heldItem;
        private GridItem stackTarget;
        private GridItem overlapNeighbor;
        private GridItem carryHighlightedItem;
        private GridInteractable focusedInteractable;

        private Vector2Int pendingCell;
        private bool pendingCellIsValid;

        private bool IsCarrying => heldItem != null;

        private void Awake()
        {
            forwardTileTracker = GetComponent<PlayerForwardTileTracker>();

            if (handTransform == null)
                Debug.LogWarning("[PlayerInteractionController] handTransform is not assigned.", this);
        }

        private void Update()
        {
            if (IsCarrying)
                UpdateCarryState();
            else
                UpdateIdleState();
        }

        private void UpdateIdleState()
        {
            if (GridMap.Instance == null) return;

            // The forward tile is the sole interaction target.
            TileHighlighter forwardTile = forwardTileTracker.ForwardTile;

            // Check for a stationary interactable.
            // Primary: use the tile's registered cell (tile-aligned props).
            // Fallback: project forward by one cell from the player's position, so
            // interactables that sit between tile centres (or on tiles with no TileHighlighter)
            // are still reachable — e.g. the store chest.
            GridInteractable interactableCandidate = null;
            if (forwardTile != null)
                interactableCandidate = GridMap.Instance.GetInteractable(forwardTile.RegisteredCell);

            if (interactableCandidate == null)
            {
                Vector2Int forwardCell = GridMap.Instance.WorldToCell(
                    transform.position + transform.forward * GridMap.CellSize);
                interactableCandidate = GridMap.Instance.GetInteractable(forwardCell);
            }

            if (interactableCandidate != focusedInteractable)
            {
                focusedInteractable?.SetFocused(false);
                focusedInteractable = interactableCandidate;
                focusedInteractable?.SetFocused(true);

                // Clear any item feedback when an interactable takes focus.
                if (focusedInteractable != null && nearestItem != null)
                {
                    nearestItem.SetNearbyFeedback(false);
                    nearestItem = null;
                }
            }

            // Only check for a GridItem if no interactable occupies the forward tile.
            if (focusedInteractable != null)
                return;

            GridItem candidate = forwardTile != null
                ? GridMap.Instance.GetItem(forwardTile.RegisteredCell)
                : null;

            // Surface the stacked item as the pickup target — the host is unreachable while occupied.
            if (candidate != null && candidate.HasStackedItem)
                candidate = candidate.StackedItem;

            if (candidate == nearestItem)
                return;

            nearestItem?.SetNearbyFeedback(false);
            nearestItem = candidate;
            nearestItem?.SetNearbyFeedback(true);
        }

        private void OnDisable()
        {
            UpdateCarryHighlight(null);
            nearestItem?.SetNearbyFeedback(false);
            nearestItem = null;
        }

        private void UpdateCarryState()
        {
            if (GridMap.Instance == null) return;

            heldItem.FollowHand(handTransform);

            TileHighlighter forwardTile = forwardTileTracker.ForwardTile;
            pendingCellIsValid = forwardTile != null && GridMap.Instance.IsCellFree(forwardTile.RegisteredCell);
            if (pendingCellIsValid)
                pendingCell = forwardTile.RegisteredCell;

            // Renderer-bounds overflow check: the cell may be free in the registry but a
            // neighbour's mesh physically extends into it, causing visual interpenetration.
            overlapNeighbor = null;
            if (pendingCellIsValid)
            {
                overlapNeighbor = FindOverlappingNeighbor(forwardTile);
                if (overlapNeighbor != null)
                    pendingCellIsValid = false;
            }

            // Stack target check.
            GridItem hostCandidate = forwardTile != null
                ? GridMap.Instance.GetItem(forwardTile.RegisteredCell)
                : null;

            bool heldCanStack   = heldItem.ItemData != null && heldItem.ItemData.canStack;
            bool hostCanReceive = hostCandidate != null
                                  && !hostCandidate.HasStackedItem
                                  && hostCandidate.HasStackMount
                                  && hostCandidate.ItemData != null
                                  && hostCandidate.ItemData.canBeStackedOn;

            stackTarget = (heldCanStack && hostCanReceive) ? hostCandidate : null;

            // ── Cascading carry-state highlight ──────────────────────────────
            // The tile highlight (yellow) is owned by PlayerForwardTileTracker.
            // We additionally glow the most relevant item in the forward cell:
            //   valid stack target            → glow the host
            //   overflow neighbour            → glow the offending item
            //   occupied cell (no valid stack)→ glow the occupant (or its stacked child)
            GridItem highlightTarget = null;
            if (stackTarget != null)
                highlightTarget = stackTarget;
            else if (overlapNeighbor != null)
                highlightTarget = overlapNeighbor;
            else if (hostCandidate != null)
                highlightTarget = hostCandidate.HasStackedItem ? hostCandidate.StackedItem : hostCandidate;

            UpdateCarryHighlight(highlightTarget);
        }

        /// <summary>
        /// Checks all grid-registered items in neighbouring cells to see whether any of their
        /// renderer bounds intersect the target cell's XZ footprint. Uses actual visual bounds
        /// rather than physics colliders — catches mesh overflow even when colliders are cell-sized.
        /// </summary>
        private GridItem FindOverlappingNeighbor(TileHighlighter tile)
        {
            Vector3 cellCenter = GridMap.Instance.CellToWorld(tile.RegisteredCell);
            // Shrink the footprint slightly to avoid false positives on shared boundaries.
            float halfSize = GridMap.CellSize * 0.45f;
            float minX = cellCenter.x - halfSize, maxX = cellCenter.x + halfSize;
            float minZ = cellCenter.z - halfSize, maxZ = cellCenter.z + halfSize;

            // Check items up to 2 cells away — large props rarely overflow further.
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dz = -2; dz <= 2; dz++)
                {
                    if (dx == 0 && dz == 0) continue;

                    Vector2Int neighborCell = tile.RegisteredCell + new Vector2Int(dx, dz);
                    GridItem neighbor = GridMap.Instance.GetItem(neighborCell);
                    if (neighbor == null) continue;

                    Bounds b = neighbor.GetWorldBounds();
                    if (b.max.x > minX && b.min.x < maxX &&
                        b.max.z > minZ && b.min.z < maxZ)
                        return neighbor;
                }
            }

            return null;
        }

        /// <summary>Applies or clears the nearby-glow on the item that is the current carry highlight.</summary>
        private void UpdateCarryHighlight(GridItem newTarget)
        {
            if (newTarget == carryHighlightedItem) return;
            carryHighlightedItem?.SetNearbyFeedback(false);
            carryHighlightedItem = newTarget;
            carryHighlightedItem?.SetNearbyFeedback(true);
        }

        /// <summary>
        /// Receives the Interact input action via the PlayerInput SendMessages behaviour.
        /// </summary>
        public void OnInteract(InputValue value)
        {
            if (!value.isPressed)
                return;

            // Priority 1 — pickup prompt is open: dismiss it.
            if (PickupPromptUI.Instance != null && PickupPromptUI.Instance.IsOpen)
            {
                PickupPromptUI.Instance.Hide();
                return;
            }

            // Priority 2 — store is open: close it.
            if (StoreUI.Instance != null && StoreUI.Instance.IsOpen)
            {
                StoreUI.Instance.Close();
                return;
            }

            // Priority 3 — stationary interactable on forward tile.
            if (!IsCarrying && focusedInteractable != null)
            {
                focusedInteractable.OnInteract();
                return;
            }

            // Priority 4 — GridItem pickup, with confirmation prompt for canBeStackedOn hosts.
            if (!IsCarrying && nearestItem != null)
            {
                bool needsPrompt = nearestItem.ItemData != null
                    && nearestItem.ItemData.canBeStackedOn
                    && !nearestItem.HasStackedItem;

                if (needsPrompt && PickupPromptUI.Instance != null)
                    PickupPromptUI.Instance.Show(nearestItem, onConfirm: () => PickUp(nearestItem));
                else
                    PickUp(nearestItem);

                return;
            }

            // Priority 5 — place or stack held item.
            if (IsCarrying && (pendingCellIsValid || stackTarget != null))
            {
                PlaceItem();
                return;
            }

            // Placement failed while carrying — diagnose why and surface a hint.
            if (IsCarrying)
                TryShowPlacementHint();
        }

        /// <summary>
        /// Inspects the forward tile to determine why placement/stacking failed
        /// and surfaces a human-readable hint via HintNotificationUI.
        /// </summary>
        private void TryShowPlacementHint()
        {
            if (HintNotificationUI.Instance == null || GridMap.Instance == null) return;

            // Overlap case: the cell was grid-free but a neighbour's mesh spills into it.
            if (overlapNeighbor != null)
            {
                string neighborName = overlapNeighbor.ItemData?.itemName ?? overlapNeighbor.name;
                HintNotificationUI.Instance.Show($"Too close — overlaps with {neighborName}");
                return;
            }

            TileHighlighter forwardTile = forwardTileTracker.ForwardTile;
            if (forwardTile == null) return;

            GridItem host = GridMap.Instance.GetItem(forwardTile.RegisteredCell);
            if (host == null) return;

            string heldName = heldItem.ItemData != null ? heldItem.ItemData.itemName : heldItem.name;
            string hostName = host.ItemData   != null ? host.ItemData.itemName   : host.name;

            if (host.HasStackedItem)
                HintNotificationUI.Instance.Show($"The {hostName} already has an item on top");
            else
                HintNotificationUI.Instance.Show($"Can't place here — blocked by {hostName}");
        }

        /// <summary>
        /// Called by StoreUI after a purchase — disables the item's collider and begins carrying it.
        /// OnPickedUp is safe to call on an unplaced item: IsPlaced is false so GridMap is not touched,
        /// but the collider is correctly disabled so it doesn't deflect the CharacterController.
        /// </summary>
        public void ReceiveItem(GridItem item)
        {
            item.OnPickedUp();
            heldItem = item;
            item.FollowHand(handTransform);
        }

        private void PickUp(GridItem item)
        {
            item.SetNearbyFeedback(false);
            item.OnPickedUp();
            heldItem = item;
            nearestItem = null;
        }

        private void PlaceItem()
        {
            if (stackTarget != null)
            {
                bool stacked = heldItem.OnStackedOn(stackTarget);
                stackTarget = null;
                if (!stacked) return; // host had no StackMount — keep item in hand
            }
            else
            {
                TileHighlighter forwardTile = forwardTileTracker.ForwardTile;
                Vector3 snapPosition = forwardTile != null
                    ? forwardTile.PlacementPosition
                    : GridMap.Instance.CellToWorld(pendingCell) + Vector3.up * 0.5f;

                heldItem.OnPlaced(pendingCell, snapPosition);
            }

            UpdateCarryHighlight(null);
            heldItem = null;
            pendingCellIsValid = false;
        }

        /// <summary>Receives the SendToInventory input action — sends the held item to InventoryManager.</summary>
        public void OnSendToInventory(InputValue value)
        {
            if (!value.isPressed || !IsCarrying)
                return;

            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("[PlayerInteractionController] InventoryManager.Instance is null — cannot send item to inventory.", this);
                return;
            }

            InventoryManager.Instance.AddItem(heldItem);
            UpdateCarryHighlight(null);
            heldItem = null;
            stackTarget = null;
            pendingCellIsValid = false;
        }
    }
}

