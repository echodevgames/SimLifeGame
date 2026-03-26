using System;
using System.Collections.Generic;
using SeedyRoots.Grid;
using TMPro;
using UnityEngine;

namespace SeedyRoots.Core
{
    /// <summary>
    /// Singleton that holds all items the player has sent to inventory via the SendToInventory action.
    /// Exposes a read-only list for future retrieval/equip phases.
    /// UI feedback this phase is a placeholder count label only.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI countLabel;

        private readonly List<GridItem> items = new List<GridItem>();

        /// <summary>Read-only view of the current inventory contents.</summary>
        public IReadOnlyList<GridItem> Items => items;

        /// <summary>Fires whenever an item is added to the inventory.</summary>
        public event Action<GridItem> OnItemAdded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[InventoryManager] Duplicate instance detected — destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (countLabel == null)
                Debug.LogWarning("[InventoryManager] countLabel is not assigned — count display will be skipped.", this);

            RefreshLabel();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Adds item to the inventory list and refreshes the count label.</summary>
        public void AddItem(GridItem item)
        {
            if (item == null) return;

            items.Add(item);
            item.gameObject.SetActive(false);
            RefreshLabel();
            OnItemAdded?.Invoke(item);

            Debug.Log($"[InventoryManager] Added '{item.name}'. Total: {items.Count}");
        }

        /// <summary>Removes item from the inventory list (for future retrieval/equip phases).</summary>
        public void RemoveItem(GridItem item)
        {
            if (item == null) return;

            if (items.Remove(item))
                RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (countLabel != null)
                countLabel.text = $"Inventory: {items.Count}";
        }
    }
}
