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
    }
}
