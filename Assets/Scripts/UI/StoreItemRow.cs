using System;
using SeedyRoots.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Drives one subcategory row in the store panel.
    /// Each row represents one ItemSubcategoryData — it shows the subcategory name and icon
    /// on the left, and variant arrow buttons in the centre that cycle through ItemData variants.
    /// The Buy button fires the purchase callback supplied by StoreUI.
    /// All serialized fields must be wired inside the StoreItemRow prefab.
    /// </summary>
    public class StoreItemRow : MonoBehaviour
    {
        [SerializeField] private Image subcategoryIcon;
        [SerializeField] private TextMeshProUGUI subcategoryLabel;
        [SerializeField] private Button variantPrevButton;
        [SerializeField] private Button variantNextButton;
        [SerializeField] private TextMeshProUGUI variantLabel;
        [SerializeField] private Image variantIcon;
        [SerializeField] private Button buyButton;

        private ItemSubcategoryData subcategory;
        private Action<ItemData> onPurchase;
        private int currentVariantIndex;

        /// <summary>
        /// Initializes this row with its subcategory data and the purchase callback from StoreUI.
        /// Must be called once after Instantiate, before the row is displayed.
        /// </summary>
        public void Initialize(ItemSubcategoryData subcategoryData, Action<ItemData> purchaseCallback)
        {
            subcategory = subcategoryData;
            onPurchase = purchaseCallback;

            if (subcategoryLabel != null)
                subcategoryLabel.text = subcategory.displayName;

            if (subcategoryIcon != null)
                subcategoryIcon.sprite = subcategory.icon;

            currentVariantIndex = 0;
            RefreshVariant();

            if (variantPrevButton != null)
                variantPrevButton.onClick.AddListener(OnVariantPrev);

            if (variantNextButton != null)
                variantNextButton.onClick.AddListener(OnVariantNext);

            if (buyButton != null)
                buyButton.onClick.AddListener(OnBuy);
        }

        private void OnVariantPrev()
        {
            if (currentVariantIndex > 0)
            {
                currentVariantIndex--;
                RefreshVariant();
            }
        }

        private void OnVariantNext()
        {
            if (currentVariantIndex < subcategory.variants.Count - 1)
            {
                currentVariantIndex++;
                RefreshVariant();
            }
        }

        private void OnBuy()
        {
            ItemData current = subcategory.variants[currentVariantIndex];
            if (current != null)
                onPurchase?.Invoke(current);
        }

        private void RefreshVariant()
        {
            ItemData current = subcategory.variants[currentVariantIndex];

            if (variantLabel != null)
                variantLabel.text = current.itemName;

            if (variantIcon != null)
            {
                variantIcon.sprite = current.icon;

                // The prefab sets alpha to 0 as a safe default so empty slots are invisible.
                // Restore full opacity when a sprite is assigned, hide when there is none.
                variantIcon.color = current.icon != null
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0f);
            }

            // Disable arrows at the bounds so the player cannot cycle past the list ends.
            if (variantPrevButton != null)
                variantPrevButton.interactable = currentVariantIndex > 0;

            if (variantNextButton != null)
                variantNextButton.interactable = currentVariantIndex < subcategory.variants.Count - 1;
        }
    }
}
