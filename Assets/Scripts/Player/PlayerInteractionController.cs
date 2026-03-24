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
            // The forward tile is the sole interaction target.
            TileHighlighter forwardTile = forwardTileTracker.ForwardTile;

            // Check for a stationary interactable first.
            GridInteractable interactableCandidate = forwardTile != null
                ? GridMap.Instance.GetInteractable(forwardTile.RegisteredCell)
                : null;

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

            if (candidate == nearestItem)
                return;

            nearestItem?.SetNearbyFeedback(false);
            nearestItem = candidate;
            nearestItem?.SetNearbyFeedback(true);
        }

        private void UpdateCarryState()
        {
            heldItem.FollowHand(handTransform);

            TileHighlighter forwardTile = forwardTileTracker.ForwardTile;
            pendingCellIsValid = forwardTile != null && GridMap.Instance.IsCellFree(forwardTile.RegisteredCell);
            if (pendingCellIsValid)
                pendingCell = forwardTile.RegisteredCell;
        }

        /// <summary>
        /// Receives the Interact input action via the PlayerInput SendMessages behaviour.
        /// </summary>
        public void OnInteract(InputValue value)
        {
            if (!value.isPressed)
                return;

            // Priority 1 — store is open: close it.
            if (StoreUI.Instance != null && StoreUI.Instance.IsOpen)
            {
                StoreUI.Instance.Close();
                return;
            }

            // Priority 2 — stationary interactable on forward tile.
            if (!IsCarrying && focusedInteractable != null)
            {
                focusedInteractable.OnInteract();
                return;
            }

            // Priority 3 — existing GridItem pickup / place logic.
            if (!IsCarrying && nearestItem != null)
                PickUp(nearestItem);
            else if (IsCarrying && pendingCellIsValid)
                PlaceItem();
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
            TileHighlighter forwardTile = forwardTileTracker.ForwardTile;
            Vector3 snapPosition = forwardTile != null
                ? forwardTile.PlacementPosition
                : GridMap.Instance.CellToWorld(pendingCell) + Vector3.up * 0.5f;

            heldItem.OnPlaced(pendingCell, snapPosition);
            heldItem = null;
            pendingCellIsValid = false;
        }
    }
}

