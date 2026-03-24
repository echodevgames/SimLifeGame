using SeedyRoots.UI;
using UnityEngine;

namespace SeedyRoots.Grid
{
    /// <summary>
    /// Stationary interactable placed on a grid tile.
    /// When the player faces it and presses Interact the global store panel opens.
    /// The item catalog is owned by StoreUI via ItemCatalog — this chest holds no item data.
    /// </summary>
    public class ChestInteractable : GridInteractable
    {
        /// <summary>Opens the global store panel.</summary>
        public override void OnInteract()
        {
            if (StoreUI.Instance == null)
            {
                Debug.LogWarning("[ChestInteractable] StoreUI.Instance is null — ensure StoreUI is in the scene.", this);
                return;
            }

            StoreUI.Instance.Open();
        }

        /// <summary>Visual feedback stub — chest outline/glow to be added in a later visual pass.</summary>
        public override void SetFocused(bool focused) { }
    }
}
