using SeedyRoots.Core;
using SeedyRoots.Grid;
using UnityEngine;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Listens for InventoryManager.OnItemAdded and fires a HintNotificationUI toast
    /// whenever an item is added to the player's inventory.
    /// </summary>
    public class InventoryToastUI : MonoBehaviour
    {
        private void Start()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("[InventoryToastUI] InventoryManager.Instance is null — cannot subscribe to OnItemAdded.", this);
                return;
            }

            InventoryManager.Instance.OnItemAdded += OnItemAdded;
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnItemAdded -= OnItemAdded;
        }

        private void OnItemAdded(GridItem item)
        {
            if (HintNotificationUI.Instance == null) return;

            string displayName = item.ItemData?.itemName ?? item.name;
            HintNotificationUI.Instance.Show($"{displayName} added to inventory");
        }
    }
}
