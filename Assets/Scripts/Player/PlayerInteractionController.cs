using SeedyRoots.Grid;
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
            // Check whether a GridItem is registered on its cell.
            TileHighlighter forwardTile = forwardTileTracker.ForwardTile;
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

            if (!IsCarrying && nearestItem != null)
                PickUp(nearestItem);
            else if (IsCarrying && pendingCellIsValid)
                PlaceItem();
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

