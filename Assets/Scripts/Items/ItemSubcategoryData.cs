using System.Collections.Generic;
using UnityEngine;

namespace SeedyRoots.Items
{
    /// <summary>
    /// ScriptableObject representing one subcategory row in the store.
    /// Holds a display name, an icon, and the ordered list of item variants
    /// the player cycles through with the per-row arrow buttons.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemSubcategoryData", menuName = "SeedyRoots/ItemSubcategoryData")]
    public class ItemSubcategoryData : ScriptableObject
    {
        /// <summary>Stable, uppercase identifier — e.g. "TOOL_BIGTOOL". Set once, never changed.</summary>
        public string subcategoryId;

        /// <summary>Display name shown in the store row label — e.g. "Big Tools".</summary>
        public string displayName;

        /// <summary>Icon shown in the subcategory section of the store row.</summary>
        public Sprite icon;

        /// <summary>Ordered list of item variants cycled by the row's arrow buttons.</summary>
        public List<ItemData> variants = new List<ItemData>();
    }
}
