using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SeedyRoots.Grid;
using SeedyRoots.Items;

namespace SeedyRoots.Editor
{
    /// <summary>
    /// Editor window that reads the canonical item catalog CSV and updates every
    /// matching ItemData ScriptableObject asset in the project.
    ///
    /// CSV column layout (0-based) — auto-detected from header row:
    ///   #, itemId, itemName, category, subcategory, base_cost, [...], asset_file, icon_file, notes,
    ///   can_be_stacked_on, can_stack
    ///
    /// Column indices are resolved by header name so the importer is resilient to
    /// extra columns (e.g. category_weight, rarity_score, cost_tier, final_cost).
    /// The cost column used is base_cost; final_cost formulas are ignored.
    /// </summary>
    public class ItemCatalogImporter : EditorWindow
    {
        private TextAsset csvAsset;

        private Vector2 scrollPosition;
        private List<ImportResult> lastResults = new List<ImportResult>();
        private bool showResults;

        [MenuItem("SeedyRoots/Item Catalog Importer")]
        public static void ShowWindow()
        {
            ItemCatalogImporter window = GetWindow<ItemCatalogImporter>("Item Catalog Importer");
            window.minSize = new Vector2(420f, 320f);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Item Catalog Importer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Reads the catalog CSV and updates every matching ItemData asset.\n" +
                "Assets are matched by their filename (asset_file column).\n" +
                "Icons are located by filename across all subfolders of Assets/Items/Icons/.",
                MessageType.Info);

            EditorGUILayout.Space(6f);

            csvAsset = (TextAsset)EditorGUILayout.ObjectField(
                "Catalog CSV", csvAsset, typeof(TextAsset), false);

            EditorGUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(csvAsset == null))
            {
                if (GUILayout.Button("Import — Update All ItemData Assets", GUILayout.Height(32f)))
                    RunImport();
            }

            if (csvAsset == null)
                EditorGUILayout.HelpBox("Assign the CSV asset above to enable import.", MessageType.Warning);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Prefab Wiring", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Iterates every ItemData asset, follows its prefab reference, and sets\n" +
                "GridItem.itemData on that prefab. Run this once after the catalog import.",
                MessageType.Info);

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("Wire GridItem.itemData on All Prefabs", GUILayout.Height(32f)))
                WireGridItemReferences();

            if (showResults && lastResults.Count > 0)
                DrawResults();
        }

        // ── Prefab Wiring ─────────────────────────────────────────────────────

        /// <summary>
        /// Iterates every ItemData asset in the project, follows its prefab field,
        /// finds the GridItem component on the prefab root, and sets itemData on it.
        /// Multiple ItemData assets may point to the same prefab (variant rows in the
        /// catalog share a prefab); in that case the last processed ItemData wins — run
        /// the catalog import first so the correct one is current.
        /// </summary>
        [MenuItem("SeedyRoots/Wire GridItem ItemData on Prefabs")]
        public static void WireGridItemReferences()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            int wired   = 0;
            int skipped = 0;

            foreach (string guid in guids)
            {
                string itemDataPath = AssetDatabase.GUIDToAssetPath(guid);
                ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(itemDataPath);
                if (itemData == null) continue;

                if (itemData.prefab == null)
                {
                    Debug.LogWarning($"[ItemCatalogImporter] {itemData.name}: prefab field is null — skipped.", itemData);
                    skipped++;
                    continue;
                }

                string prefabPath = AssetDatabase.GetAssetPath(itemData.prefab);
                if (string.IsNullOrEmpty(prefabPath))
                {
                    Debug.LogWarning($"[ItemCatalogImporter] {itemData.name}: could not resolve prefab path — skipped.", itemData);
                    skipped++;
                    continue;
                }

                // Load the prefab asset and find GridItem on the root (or any descendant).
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                GridItem gridItem = prefabRoot.GetComponentInChildren<GridItem>(includeInactive: true);

                if (gridItem == null)
                {
                    Debug.LogWarning($"[ItemCatalogImporter] {prefabPath}: no GridItem component found — skipped.", itemData);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    skipped++;
                    continue;
                }

                SerializedObject so = new SerializedObject(gridItem);
                SerializedProperty prop = so.FindProperty("itemData");
                prop.objectReferenceValue = itemData;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                wired++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ItemCatalogImporter] Prefab wiring complete — {wired} wired, {skipped} skipped.");
            EditorUtility.DisplayDialog("Prefab Wiring Complete", $"Wired: {wired}\nSkipped: {skipped}", "OK");
        }

        // ── Import ────────────────────────────────────────────────────────────

        private void RunImport()
        {
            lastResults.Clear();

            // Build lookup tables.
            Dictionary<string, string> assetPathByFilename = BuildAssetPathLookup();
            Dictionary<string, string> iconPathByFilename  = BuildIconPathLookup();

            string[] lines = csvAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                Debug.LogError("[ItemCatalogImporter] CSV has no data rows.");
                return;
            }

            // ── Resolve column indices from header ────────────────────────────
            string[] headers = ParseCsvRow(lines[0]);
            int colItemId        = IndexOf(headers, "itemId");
            int colItemName      = IndexOf(headers, "itemName");
            int colBaseCost      = IndexOf(headers, "base_cost");
            if (colBaseCost < 0) colBaseCost = IndexOf(headers, "cost");   // fallback for original CSV
            int colAssetFile     = IndexOf(headers, "asset_file");
            int colIconFile      = IndexOf(headers, "icon_file");
            int colStackedOn     = IndexOf(headers, "can_be_stacked_on");
            int colCanStack      = IndexOf(headers, "can_stack");
            int colCategory      = IndexOf(headers, "category");
            int colSubcategory   = IndexOf(headers, "subcategory");

            if (colItemId < 0 || colAssetFile < 0)
            {
                Debug.LogError("[ItemCatalogImporter] CSV is missing required columns (itemId, asset_file). Check the header row.");
                return;
            }

            int updated = 0;
            int skipped = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = ParseCsvRow(line);

                string assetFilename = SafeGet(cols, colAssetFile);
                string iconFilename  = SafeGet(cols, colIconFile);

                if (string.IsNullOrEmpty(assetFilename))
                {
                    lastResults.Add(new ImportResult(i + 1, "—", false, "asset_file column is empty — skipped."));
                    skipped++;
                    continue;
                }

                if (!assetPathByFilename.TryGetValue(assetFilename, out string assetPath))
                {
                    lastResults.Add(new ImportResult(i + 1, assetFilename, false, "ItemData asset not found in project."));
                    skipped++;
                    continue;
                }

                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                if (item == null)
                {
                    lastResults.Add(new ImportResult(i + 1, assetFilename, false, "Asset exists but could not be loaded as ItemData."));
                    skipped++;
                    continue;
                }

                // ── Write fields ──────────────────────────────────────────────
                item.itemId   = SafeGet(cols, colItemId);
                item.itemName = SafeGet(cols, colItemName);

                // base_cost is a plain integer; final_cost may be a formula — skip it.
                string costStr = SafeGet(cols, colBaseCost);
                if (int.TryParse(costStr, out int cost))
                    item.cost = cost;

                if (colStackedOn >= 0)
                    item.canBeStackedOn = SafeGet(cols, colStackedOn).Equals("TRUE", StringComparison.OrdinalIgnoreCase);

                if (colCanStack >= 0)
                    item.canStack = SafeGet(cols, colCanStack).Equals("TRUE", StringComparison.OrdinalIgnoreCase);

                if (colCategory >= 0)
                    item.category = SafeGet(cols, colCategory);

                if (colSubcategory >= 0)
                    item.subcategory = SafeGet(cols, colSubcategory);

                // ── Icon ──────────────────────────────────────────────────────
                string iconWarning = string.Empty;
                if (!string.IsNullOrEmpty(iconFilename))
                {
                    if (iconPathByFilename.TryGetValue(iconFilename, out string iconPath))
                    {
                        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                        if (icon != null)
                            item.icon = icon;
                        else
                            iconWarning = $" | Icon at wrong type: {iconPath}";
                    }
                    else
                    {
                        iconWarning = $" | Icon not found: {iconFilename}";
                    }
                }

                EditorUtility.SetDirty(item);
                updated++;

                lastResults.Add(new ImportResult(i + 1, assetFilename, true,
                    $"OK — cost:{item.cost}  cat:{item.category}  stackedOn:{item.canBeStackedOn}  canStack:{item.canStack}{iconWarning}"));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            showResults = true;
            Debug.Log($"[ItemCatalogImporter] Done — {updated} updated, {skipped} skipped.");
        }

        // ── CSV parsing ───────────────────────────────────────────────────────

        /// <summary>
        /// Splits a single CSV row respecting double-quoted fields that may contain commas.
        /// Surrounding quotes are stripped from each field.
        /// </summary>
        private static string[] ParseCsvRow(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Escaped quote inside a quoted field ("").
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString().Trim());
            return fields.ToArray();
        }

        private static int IndexOf(string[] headers, string name)
        {
            for (int i = 0; i < headers.Length; i++)
                if (headers[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        private static string SafeGet(string[] cols, int index)
        {
            return (index >= 0 && index < cols.Length) ? cols[index].Trim() : string.Empty;
        }

        // ── Lookups ───────────────────────────────────────────────────────────

        /// <summary>Returns a dictionary mapping asset filename → full asset path for all ItemData assets.</summary>
        private static Dictionary<string, string> BuildAssetPathLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string filename = System.IO.Path.GetFileName(path);
                if (!lookup.ContainsKey(filename))
                    lookup[filename] = path;
            }
            return lookup;
        }

        /// <summary>Returns a dictionary mapping icon filename → full asset path, searching under Assets/Items/Icons/.</summary>
        private static Dictionary<string, string> BuildIconPathLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Items/Icons" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string filename = System.IO.Path.GetFileName(path);
                if (!lookup.ContainsKey(filename))
                    lookup[filename] = path;
            }
            return lookup;
        }

        // ── Results UI ────────────────────────────────────────────────────────

        private void DrawResults()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            foreach (ImportResult r in lastResults)
            {
                Color prev = GUI.contentColor;
                GUI.contentColor = r.success ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.5f, 0.4f);
                EditorGUILayout.LabelField($"Row {r.row:D2}  {r.filename}", r.message, EditorStyles.miniLabel);
                GUI.contentColor = prev;
            }

            EditorGUILayout.EndScrollView();
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private readonly struct ImportResult
        {
            public readonly int    row;
            public readonly string filename;
            public readonly bool   success;
            public readonly string message;

            public ImportResult(int row, string filename, bool success, string message)
            {
                this.row      = row;
                this.filename = filename;
                this.success  = success;
                this.message  = message;
            }
        }
    }
}
