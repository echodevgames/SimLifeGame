using UnityEngine;

namespace SeedyRoots.Items
{
    /// <summary>
    /// ScriptableObject defining a single purchasable item.
    /// Holds the display name, icon, spawn prefab, and cost field for the economy system.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "SeedyRoots/ItemData")]
    public class ItemData : ScriptableObject
    {
        /// <summary>Stable, uppercase identifier — e.g. "CONTAINER_BUCKET_01". Set once, never changed.</summary>
        public string itemId;

        public string itemName;
        public Sprite icon;
        public GameObject prefab;
        public int cost;
        public string category;
        public string subcategory;

        /// <summary>True if other items can be placed on top of this item via its StackMount child transform.</summary>
        public bool canBeStackedOn;

        /// <summary>True if this item is allowed to be placed on top of a canBeStackedOn host.</summary>
        public bool canStack;
    }
}
