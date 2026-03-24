using UnityEngine;

namespace SeedyRoots.Grid
{
    /// <summary>
    /// Abstract base class for stationary grid objects that respond to Interact without being carriable.
    /// Registers its cell with GridMap on Start using the same lifecycle as TileHighlighter.
    /// </summary>
    public abstract class GridInteractable : MonoBehaviour
    {
        public Vector2Int RegisteredCell { get; private set; }

        protected virtual void Start()
        {
            if (GridMap.Instance == null)
            {
                Debug.LogWarning("[GridInteractable] GridMap.Instance is null — cannot register interactable.", this);
                return;
            }

            RegisteredCell = GridMap.Instance.WorldToCell(transform.position);
            GridMap.Instance.RegisterInteractable(RegisteredCell, this);
        }

        protected virtual void OnDestroy()
        {
            if (GridMap.Instance != null)
                GridMap.Instance.UnregisterInteractable(RegisteredCell);
        }

        /// <summary>Called when the player presses Interact while this object is the forward target.</summary>
        public abstract void OnInteract();

        /// <summary>Called when this becomes or ceases to be the active forward interaction target.</summary>
        public virtual void SetFocused(bool focused) { }
    }
}
