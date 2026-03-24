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
    }
}
