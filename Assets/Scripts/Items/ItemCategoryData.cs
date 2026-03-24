using System.Collections.Generic;
using UnityEngine;

namespace SeedyRoots.Items
{
    /// <summary>
    /// ScriptableObject representing one top-level store category.
    /// The store's category carousel cycles through all categories in the ItemCatalog.
    /// Each category holds an ordered list of subcategories displayed as rows.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemCategoryData", menuName = "SeedyRoots/ItemCategoryData")]
    public class ItemCategoryData : ScriptableObject
    {
        /// <summary>Stable, uppercase identifier — e.g. "TOOL". Set once, never changed.</summary>
        public string categoryId;

        /// <summary>Display name shown in the category carousel label — e.g. "Tools".</summary>
        public string displayName;

        /// <summary>Icon representing this category in the store header.</summary>
        public Sprite icon;

        /// <summary>Ordered list of subcategories displayed as rows when this category is active.</summary>
        public List<ItemSubcategoryData> subcategories = new List<ItemSubcategoryData>();
    }
}
