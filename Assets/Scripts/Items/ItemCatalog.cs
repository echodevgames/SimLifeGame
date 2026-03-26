using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeedyRoots.Items
{
    /// <summary>
    /// Global ScriptableObject registry that is the single source of truth for the store.
    /// StoreUI holds a serialized reference to the one ItemCatalog asset in the project.
    /// Add new categories here to extend the store without touching any code.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "SeedyRoots/ItemCatalog")]
    public class ItemCatalog : ScriptableObject
    {
        /// <summary>All categories available in the store, in carousel display order.</summary>
        public List<ItemCategoryData> categories = new List<ItemCategoryData>();

        private Dictionary<string, ItemData> itemById;

        /// <summary>Returns the ItemData for the given itemId, or null if not found. Cache is built on first call.</summary>
        public ItemData GetItemById(string itemId)
        {
            if (itemById == null)
            {
                itemById = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
                foreach (ItemCategoryData cat in categories)
                    foreach (ItemSubcategoryData sub in cat.subcategories)
                        foreach (ItemData item in sub.variants)
                            if (item != null && !string.IsNullOrEmpty(item.itemId))
                                itemById.TryAdd(item.itemId, item);
            }

            itemById.TryGetValue(itemId, out ItemData result);
            return result;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Invalidate the cache when the asset changes in the Editor.
            itemById = null;
        }
#endif
    }
}
