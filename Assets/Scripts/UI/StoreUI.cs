using System.Collections.Generic;
using SeedyRoots.Core;
using SeedyRoots.Grid;
using SeedyRoots.Items;
using SeedyRoots.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Singleton screen-space UI controller for the store panel.
    /// The store is driven by a global ItemCatalog — any ChestInteractable opens the full catalog.
    /// Categories are navigated with prev/next arrow buttons at the top of the panel.
    /// Each visible subcategory row (StoreItemRow) handles variant cycling internally.
    /// </summary>
    public class StoreUI : MonoBehaviour
    {
        public static StoreUI Instance { get; private set; }

        [SerializeField] private GameObject storePanel;
        [SerializeField] private Button closeButton;

        [Header("Catalog")]
        [SerializeField] private ItemCatalog catalog;

        [Header("Category Carousel")]
        [SerializeField] private TextMeshProUGUI categoryLabel;
        [SerializeField] private Button categoryPrevButton;
        [SerializeField] private Button categoryNextButton;

        [Header("Subcategory Rows")]
        [SerializeField] private Transform subcategoryRowContainer;
        [SerializeField] private GameObject subcategoryRowPrefab;

        public bool IsOpen { get; private set; }

        private int currentCategoryIndex;
        private readonly List<StoreItemRow> activeRows = new();
        private PlayerInteractionController cachedPlayerController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[StoreUI] Duplicate instance detected — destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (storePanel == null)              Debug.LogWarning("[StoreUI] storePanel is not assigned.", this);
            if (catalog == null)                 Debug.LogWarning("[StoreUI] catalog is not assigned.", this);
            if (categoryLabel == null)           Debug.LogWarning("[StoreUI] categoryLabel is not assigned.", this);
            if (categoryPrevButton == null)      Debug.LogWarning("[StoreUI] categoryPrevButton is not assigned.", this);
            if (categoryNextButton == null)      Debug.LogWarning("[StoreUI] categoryNextButton is not assigned.", this);
            if (subcategoryRowContainer == null) Debug.LogWarning("[StoreUI] subcategoryRowContainer is not assigned.", this);
            if (subcategoryRowPrefab == null)    Debug.LogWarning("[StoreUI] subcategoryRowPrefab is not assigned.", this);
            if (closeButton == null)             Debug.LogWarning("[StoreUI] closeButton is not assigned.", this);
            if (CurrencyManager.Instance == null) Debug.LogWarning("[StoreUI] CurrencyManager.Instance is null — purchase affordability checks will be skipped.", this);

            if (storePanel != null)
                storePanel.SetActive(false);

            IsOpen = false;

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (categoryPrevButton != null)
                categoryPrevButton.onClick.AddListener(OnCategoryPrev);

            if (categoryNextButton != null)
                categoryNextButton.onClick.AddListener(OnCategoryNext);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Opens the store panel starting at the first category.
        /// Caches the PlayerInteractionController reference for use in PurchaseItem.
        /// </summary>
        public void Open()
        {
            cachedPlayerController = FindFirstObjectByType<PlayerInteractionController>();

            if (cachedPlayerController == null)
            {
                Debug.LogWarning("[StoreUI] PlayerInteractionController not found in scene.", this);
                return;
            }

            if (catalog == null || catalog.categories == null || catalog.categories.Count == 0)
            {
                Debug.LogWarning("[StoreUI] ItemCatalog is not assigned or contains no categories.", this);
                return;
            }

            currentCategoryIndex = 0;
            RefreshCategory();
            storePanel.SetActive(true);
            IsOpen = true;
        }

        /// <summary>Hides the panel and destroys all active subcategory rows.</summary>
        public void Close()
        {
            ClearRows();
            storePanel.SetActive(false);
            IsOpen = false;
        }

        private void OnCategoryPrev()
        {
            int count = catalog.categories.Count;
            currentCategoryIndex = (currentCategoryIndex - 1 + count) % count;
            RefreshCategory();
        }

        private void OnCategoryNext()
        {
            currentCategoryIndex = (currentCategoryIndex + 1) % catalog.categories.Count;
            RefreshCategory();
        }

        private void RefreshCategory()
        {
            ClearRows();

            ItemCategoryData category = catalog.categories[currentCategoryIndex];

            if (categoryLabel != null)
                categoryLabel.text = category.displayName;

            foreach (ItemSubcategoryData subcategory in category.subcategories)
            {
                if (subcategory == null || subcategory.variants == null || subcategory.variants.Count == 0)
                    continue;

                GameObject rowGo = Instantiate(subcategoryRowPrefab, subcategoryRowContainer);
                StoreItemRow row = rowGo.GetComponent<StoreItemRow>();

                if (row == null)
                {
                    Debug.LogError("[StoreUI] subcategoryRowPrefab is missing a StoreItemRow component.", this);
                    Destroy(rowGo);
                    continue;
                }

                row.Initialize(subcategory, PurchaseItem);
                activeRows.Add(row);
            }
        }

        private void PurchaseItem(ItemData item)
        {
            if (item.prefab == null)
            {
                Debug.LogError($"[StoreUI] ItemData '{item.itemName}' has no prefab assigned.", this);
                return;
            }

            if (CurrencyManager.Instance != null && !CurrencyManager.Instance.TrySpend(item.cost))
            {
                HintNotificationUI.Instance?.Show("Not enough money");
                return;
            }

            GameObject instance = Instantiate(item.prefab, Vector3.zero, Quaternion.identity);
            GridItem gridItem = instance.GetComponent<GridItem>();

            if (gridItem == null)
            {
                Debug.LogError($"[StoreUI] Prefab for '{item.itemName}' has no GridItem component — cannot purchase.", this);
                Destroy(instance);
                return;
            }

            cachedPlayerController.ReceiveItem(gridItem);
            Close();
        }

        private void ClearRows()
        {
            foreach (StoreItemRow row in activeRows)
            {
                if (row != null)
                    Destroy(row.gameObject);
            }

            activeRows.Clear();
        }
    }
}
