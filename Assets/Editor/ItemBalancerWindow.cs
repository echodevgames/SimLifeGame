using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SeedyRoots.Items;

namespace SeedyRoots.Editor
{
    /// <summary>
    /// Editor window for real-time browsing and live editing of all ItemData assets.
    /// Supports filtering by category, search by name, and per-field inline editing.
    /// Changes are written directly to the ScriptableObject assets — no reimport required.
    /// </summary>
    public class ItemBalancerWindow : EditorWindow
    {
        // ── State ─────────────────────────────────────────────────────────────

        private List<ItemData> allItems     = new List<ItemData>();
        private List<ItemData> filteredItems = new List<ItemData>();

        private string searchQuery  = string.Empty;
        private string categoryFilter = "All";
        private List<string> categories = new List<string>();

        private Vector2 scrollPosition;
        private bool dirty;

        // ── Column widths ─────────────────────────────────────────────────────

        private const float ColIcon      = 40f;
        private const float ColName      = 160f;
        private const float ColCategory  = 100f;
        private const float ColCost      = 70f;
        private const float ColStackedOn = 96f;
        private const float ColCanStack  = 76f;
        private const float ColPing      = 46f;

        [MenuItem("SeedyRoots/Item Balancer")]
        public static void ShowWindow()
        {
            ItemBalancerWindow window = GetWindow<ItemBalancerWindow>("Item Balancer");
            window.minSize = new Vector2(660f, 400f);
        }

        private void OnEnable()
        {
            LoadAllItems();
        }

        // ── GUI ───────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.Space(2f);
            DrawColumnHeaders();
            EditorGUILayout.Space(2f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawItemRows();
            EditorGUILayout.EndScrollView();

            DrawFooter();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Search
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50f));
            string newQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(180f));
            if (newQuery != searchQuery)
            {
                searchQuery = newQuery;
                ApplyFilters();
            }

            GUILayout.Space(12f);

            // Category filter
            EditorGUILayout.LabelField("Category:", GUILayout.Width(62f));
            int currentIndex = categories.IndexOf(categoryFilter);
            if (currentIndex < 0) currentIndex = 0;
            int newIndex = EditorGUILayout.Popup(currentIndex, categories.ToArray(),
                EditorStyles.toolbarPopup, GUILayout.Width(120f));
            if (newIndex != currentIndex)
            {
                categoryFilter = categories[newIndex];
                ApplyFilters();
            }

            GUILayout.FlexibleSpace();

            // Reload
            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(54f)))
                LoadAllItems();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawColumnHeaders()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(ColIcon + 4f);
            EditorGUILayout.LabelField("Item Name",      EditorStyles.boldLabel, GUILayout.Width(ColName));
            EditorGUILayout.LabelField("Category",       EditorStyles.boldLabel, GUILayout.Width(ColCategory));
            EditorGUILayout.LabelField("Cost",           EditorStyles.boldLabel, GUILayout.Width(ColCost));
            EditorGUILayout.LabelField("Stacked On",     EditorStyles.boldLabel, GUILayout.Width(ColStackedOn));
            EditorGUILayout.LabelField("Can Stack",      EditorStyles.boldLabel, GUILayout.Width(ColCanStack));
            EditorGUILayout.LabelField("",               GUILayout.Width(ColPing));
            EditorGUILayout.EndHorizontal();

            Rect r = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(r.x, r.yMax + 1f, r.width, 1f), new Color(0.3f, 0.3f, 0.3f));
            GUILayout.Space(3f);
        }

        private void DrawItemRows()
        {
            for (int i = 0; i < filteredItems.Count; i++)
            {
                ItemData item = filteredItems[i];
                if (item == null) continue;

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                // Icon preview
                if (item.icon != null)
                    GUILayout.Label(AssetPreview.GetAssetPreview(item.icon) ?? AssetPreview.GetMiniThumbnail(item),
                        GUILayout.Width(ColIcon - 4f), GUILayout.Height(20f));
                else
                    GUILayout.Label(AssetPreview.GetMiniThumbnail(item),
                        GUILayout.Width(ColIcon - 4f), GUILayout.Height(20f));

                // Name (read-only — itemName is set via CSV)
                EditorGUILayout.LabelField(item.itemName, GUILayout.Width(ColName));

                // Category derived from itemId prefix (read-only display)
                string cat = DeriveCategory(item.itemId);
                EditorGUILayout.LabelField(cat, GUILayout.Width(ColCategory));

                // Cost — editable
                int newCost = EditorGUILayout.IntField(item.cost, GUILayout.Width(ColCost));
                if (newCost != item.cost)
                    item.cost = Mathf.Max(0, newCost);

                // canBeStackedOn — editable
                bool newStackedOn = EditorGUILayout.Toggle(item.canBeStackedOn, GUILayout.Width(ColStackedOn));
                if (newStackedOn != item.canBeStackedOn)
                    item.canBeStackedOn = newStackedOn;

                // canStack — editable
                bool newCanStack = EditorGUILayout.Toggle(item.canStack, GUILayout.Width(ColCanStack));
                if (newCanStack != item.canStack)
                    item.canStack = newCanStack;

                // Ping button — select the asset in the Project window
                if (GUILayout.Button("→", GUILayout.Width(ColPing)))
                    EditorGUIUtility.PingObject(item);

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(item);
                    dirty = true;
                }
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"{filteredItems.Count} of {allItems.Count} items",
                EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();

            // Bulk cost tools
            EditorGUILayout.LabelField("Bulk adjust visible:", EditorStyles.miniLabel, GUILayout.Width(116f));

            if (GUILayout.Button("+10%", GUILayout.Width(46f)))
                AdjustCosts(1.10f);

            if (GUILayout.Button("-10%", GUILayout.Width(46f)))
                AdjustCosts(0.90f);

            GUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(!dirty))
            {
                if (GUILayout.Button("Save All", GUILayout.Width(72f)))
                    SaveAll();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadAllItems()
        {
            allItems.Clear();
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null)
                    allItems.Add(item);
            }

            allItems = allItems.OrderBy(i => i.itemId).ToList();

            // Build category list from itemId prefixes
            categories = new List<string> { "All" };
            categories.AddRange(allItems
                .Select(i => DeriveCategory(i.itemId))
                .Distinct()
                .OrderBy(c => c));

            ApplyFilters();
            dirty = false;
        }

        private void ApplyFilters()
        {
            filteredItems = allItems.Where(item =>
            {
                if (item == null) return false;

                bool matchesSearch = string.IsNullOrEmpty(searchQuery)
                    || item.itemName.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0
                    || item.itemId.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0;

                bool matchesCategory = categoryFilter == "All"
                    || DeriveCategory(item.itemId) == categoryFilter;

                return matchesSearch && matchesCategory;
            }).ToList();
        }

        private void AdjustCosts(float multiplier)
        {
            foreach (ItemData item in filteredItems)
            {
                if (item == null) continue;
                item.cost = Mathf.Max(1, Mathf.RoundToInt(item.cost * multiplier));
                EditorUtility.SetDirty(item);
            }
            dirty = true;
        }

        private void SaveAll()
        {
            AssetDatabase.SaveAssets();
            dirty = false;
            Debug.Log("[ItemBalancer] All changes saved.");
        }

        /// <summary>Derives a display category string from the itemId prefix.</summary>
        private static string DeriveCategory(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return "Unknown";
            if (itemId.StartsWith("BARREL"))    return "Barrels";
            if (itemId.StartsWith("CONTAINER")) return "Containers";
            if (itemId.StartsWith("HAY"))       return "Hay";
            if (itemId.StartsWith("SACK"))      return "Sacks";
            if (itemId.StartsWith("TOOL"))      return "Tools";
            if (itemId.StartsWith("TROUGH"))    return "Troughs";
            if (itemId.StartsWith("WELL"))      return "Wells";
            return "Other";
        }
    }
}
