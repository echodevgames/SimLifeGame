using System.Collections.Generic;
using SeedyRoots.Core;
using SeedyRoots.Grid;
using SeedyRoots.Items;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Panel controller for the full inventory screen.
    /// Subscribes directly to the ToggleInventory action on the scene's PlayerInput at Start,
    /// so it works regardless of where in the hierarchy this component lives.
    /// Groups items by ItemData, builds category tabs, and populates a grid with InventorySlot prefabs.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform categoryTabRow;
        [SerializeField] private GameObject categoryTabPrefab;
        [SerializeField] private Transform itemGrid;
        [SerializeField] private GameObject inventorySlotPrefab;

        public bool IsOpen { get; private set; }

        private readonly List<GameObject> activeTabButtons = new();
        private string selectedCategory;
        private InputAction toggleAction;

        private void Awake()
        {
            if (inventoryPanel == null)       Debug.LogWarning("[InventoryUI] inventoryPanel is not assigned.", this);
            if (categoryTabRow == null)       Debug.LogWarning("[InventoryUI] categoryTabRow is not assigned.", this);
            if (categoryTabPrefab == null)    Debug.LogWarning("[InventoryUI] categoryTabPrefab is not assigned.", this);
            if (itemGrid == null)             Debug.LogWarning("[InventoryUI] itemGrid is not assigned.", this);
            if (inventorySlotPrefab == null)  Debug.LogWarning("[InventoryUI] inventorySlotPrefab is not assigned.", this);

            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);

            IsOpen = false;
        }

        private void Start()
        {
            // Find the PlayerInput anywhere in the scene and subscribe to the action directly.
            // This decouples InventoryUI from the hierarchy — it can live anywhere.
            PlayerInput pi = FindFirstObjectByType<PlayerInput>();
            if (pi != null)
            {
                toggleAction = pi.actions.FindAction("ToggleInventory", throwIfNotFound: false);
                if (toggleAction != null)
                    toggleAction.performed += OnTogglePerformed;
                else
                    Debug.LogWarning("[InventoryUI] 'ToggleInventory' action not found in PlayerInput actions asset.", this);
            }
            else
            {
                Debug.LogWarning("[InventoryUI] No PlayerInput found in scene — ToggleInventory will not work.", this);
            }
        }

        private void OnDestroy()
        {
            if (toggleAction != null)
                toggleAction.performed -= OnTogglePerformed;
        }

        private void OnTogglePerformed(InputAction.CallbackContext ctx) => Toggle();

        /// <summary>Opens the inventory panel if no conflicting modal is open.</summary>
        public void Open()
        {
            if (StoreUI.Instance != null && StoreUI.Instance.IsOpen)
                return;

            if (PickupPromptUI.Instance != null && PickupPromptUI.Instance.IsOpen)
                return;

            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("[InventoryUI] InventoryManager.Instance is null — cannot open inventory.", this);
                return;
            }

            BuildCategoryTabs();

            if (inventoryPanel != null)
                inventoryPanel.SetActive(true);

            IsOpen = true;
        }

        /// <summary>Closes the inventory panel and clears all runtime-generated slots and tabs.</summary>
        public void Close()
        {
            ClearSlots();
            ClearTabs();

            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);

            IsOpen = false;
        }

        /// <summary>Toggles the inventory panel between open and closed.</summary>
        public void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        private void BuildCategoryTabs()
        {
            ClearTabs();

            IReadOnlyList<GridItem> items = InventoryManager.Instance.Items;

            // Collect distinct non-null categories in insertion order.
            var distinctCategories = new List<string>();
            foreach (GridItem item in items)
            {
                string cat = item.ItemData?.category;
                if (!string.IsNullOrEmpty(cat) && !distinctCategories.Contains(cat))
                    distinctCategories.Add(cat);
            }

            // Fall back to a generic "All" tab when no category data exists.
            if (distinctCategories.Count == 0)
                distinctCategories.Add("All");

            selectedCategory = distinctCategories[0];

            foreach (string category in distinctCategories)
            {
                string capturedCategory = category;

                GameObject tabGo = Instantiate(categoryTabPrefab, categoryTabRow);
                activeTabButtons.Add(tabGo);

                // Set the tab label if the prefab has a TMP component anywhere on it.
                TextMeshProUGUI tabLabel = tabGo.GetComponentInChildren<TextMeshProUGUI>();
                if (tabLabel != null)
                    tabLabel.text = category;

                // Wire button click.
                Button tabButton = tabGo.GetComponentInChildren<Button>();
                if (tabButton != null)
                    tabButton.onClick.AddListener(() => SelectCategory(capturedCategory));
            }

            PopulateGrid(selectedCategory);
        }

        private void SelectCategory(string category)
        {
            selectedCategory = category;
            PopulateGrid(category);
        }

        private void PopulateGrid(string category)
        {
            ClearSlots();

            if (InventoryManager.Instance == null || inventorySlotPrefab == null || itemGrid == null)
                return;

            // Group items in the selected category by ItemData reference.
            var countByData = new Dictionary<ItemData, int>();

            foreach (GridItem item in InventoryManager.Instance.Items)
            {
                ItemData data = item.ItemData;
                if (data == null) continue;

                // "All" pseudo-category shows everything.
                bool matches = category == "All" || data.category == category;
                if (!matches) continue;

                if (countByData.ContainsKey(data))
                    countByData[data]++;
                else
                    countByData[data] = 1;
            }

            foreach (KeyValuePair<ItemData, int> kvp in countByData)
            {
                GameObject slotGo = Instantiate(inventorySlotPrefab, itemGrid);
                InventorySlot slot = slotGo.GetComponent<InventorySlot>();
                if (slot != null)
                    slot.Initialize(kvp.Key, kvp.Value);
            }
        }

        private void ClearSlots()
        {
            if (itemGrid == null) return;

            for (int i = itemGrid.childCount - 1; i >= 0; i--)
                Destroy(itemGrid.GetChild(i).gameObject);
        }

        private void ClearTabs()
        {
            foreach (GameObject tab in activeTabButtons)
            {
                if (tab != null)
                    Destroy(tab);
            }
            activeTabButtons.Clear();
        }
    }
}
