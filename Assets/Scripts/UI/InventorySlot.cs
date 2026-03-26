using SeedyRoots.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Drives one slot in the inventory grid. Bound to the InventorySlot prefab.
    /// Call Initialize once after Instantiate to bind the slot to its ItemData and count.
    /// </summary>
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private GameObject countBadge;   // parent GO housing countLabel
        [SerializeField] private TextMeshProUGUI countLabel;

        /// <summary>Populates the slot with the given item data and stack count.</summary>
        public void Initialize(ItemData data, int count)
        {
            if (data == null) return;

            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(data.icon != null);
            }

            if (nameLabel != null)
                nameLabel.text = data.itemName;

            bool showBadge = count > 1;
            if (countBadge != null)
                countBadge.SetActive(showBadge);

            if (countLabel != null)
                countLabel.text = count.ToString();
        }
    }
}
