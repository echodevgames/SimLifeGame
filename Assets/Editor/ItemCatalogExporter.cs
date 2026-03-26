using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using SeedyRoots.Items;

namespace SeedyRoots.Editor
{
    /// <summary>
    /// Editor window that reads all ItemData assets in the project and syncs their
    /// values back into the catalog CSV file — the reverse direction of ItemCatalogImporter.
    ///
    /// On export:
    ///   • Adds any missing columns (can_stack, can_be_stacked_on) after the cost column.
    ///   • Updates those values from the live assets; all other columns are preserved verbatim.
    ///   • Appends new rows for any ItemData assets not yet listed in the CSV.
    ///   • Extra columns present in the source CSV but not in the canonical layout are
    ///     preserved and appended after the known columns.
    /// </summary>
    public class ItemCatalogExporter : EditorWindow
    {
        // Canonical column order for the output CSV.
        private static readonly string[] CanonicalColumns =
        {
            "#", "itemId", "itemName", "category", "subcategory", "cost",
            "can_stack", "can_be_stacked_on",
            "icon", "prefab", "asset_file", "icon_file", "notes"
        };

        // Columns whose values are written from ItemData assets on every export.
        private static readonly HashSet<string> ManagedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "can_stack", "can_be_stacked_on"
        };

        private TextAsset csvAsset;
        private Vector2 scrollPos;
        private List<string> previewLines = new List<string>();
        private bool previewReady;
        private int statColsAdded;
        private int statRowsUpdated;
        private int statRowsAppended;

        [MenuItem("SeedyRoots/Item Catalog Exporter")]
        public static void ShowWindow()
        {
            var win = GetWindow<ItemCatalogExporter>("Item Catalog Exporter");
            win.minSize = new Vector2(600f, 420f);
        }

        // ── GUI ───────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Item Catalog Exporter", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Reads all ItemData assets and syncs values back into the catalog CSV.\n" +
                "  • Adds missing columns (can_stack, can_be_stacked_on) to the header.\n" +
                "  • Updates those values from live assets — all other columns are untouched.\n" +
                "  • Appends rows for ItemData assets not yet listed in the CSV.",
                MessageType.Info);

            EditorGUILayout.Space(6f);
            csvAsset = (TextAsset)EditorGUILayout.ObjectField("Catalog CSV", csvAsset, typeof(TextAsset), false);
            EditorGUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(csvAsset == null))
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Preview", GUILayout.Height(30f), GUILayout.Width(140f)))
                {
                    BuildExportData(out _, out _, out _);
                    previewReady = true;
                }

                if (GUILayout.Button("Export — Write CSV", GUILayout.Height(30f)))
                    RunExport();

                EditorGUILayout.EndHorizontal();
            }

            if (csvAsset == null)
                EditorGUILayout.HelpBox("Assign the catalog CSV asset above.", MessageType.Warning);

            if (previewReady && previewLines.Count > 0)
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField(
                    $"Preview  —  {statColsAdded} column(s) added · " +
                    $"{statRowsUpdated} row(s) updated · {statRowsAppended} row(s) appended",
                    EditorStyles.boldLabel);

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

                // Header in white, modified rows in yellow, new rows in green.
                for (int i = 0; i < previewLines.Count; i++)
                {
                    Color prev = GUI.contentColor;
                    if (i == 0)
                        GUI.contentColor = Color.white;
                    else if (i > previewLines.Count - statRowsAppended - 1)
                        GUI.contentColor = new Color(0.4f, 1f, 0.4f);
                    else
                        GUI.contentColor = new Color(1f, 0.9f, 0.4f);

                    EditorGUILayout.LabelField(previewLines[i], EditorStyles.miniLabel);
                    GUI.contentColor = prev;
                }

                EditorGUILayout.EndScrollView();
            }
        }

        // ── Export ────────────────────────────────────────────────────────────

        private void RunExport()
        {
            string csvText = BuildExportData(out int colsAdded, out int rowsUpdated, out int rowsAppended);

            string path = AssetDatabase.GetAssetPath(csvAsset);
            File.WriteAllText(path, csvText, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            previewReady = true;
            Debug.Log($"[ItemCatalogExporter] Saved → {path} " +
                      $"(colsAdded:{colsAdded} updated:{rowsUpdated} appended:{rowsAppended})");
            EditorUtility.DisplayDialog("Export Complete",
                $"Columns added : {colsAdded}\n" +
                $"Rows updated  : {rowsUpdated}\n" +
                $"Rows appended : {rowsAppended}\n\n" +
                $"File: {path}", "OK");
        }

        /// <summary>
        /// Merges ItemData asset values into the CSV data, returning the output CSV text.
        /// Also refreshes the preview list and the stat fields.
        /// </summary>
        private string BuildExportData(out int colsAdded, out int rowsUpdated, out int rowsAppended)
        {
            colsAdded   = 0;
            rowsUpdated = 0;
            rowsAppended = 0;
            previewLines.Clear();

            // ── 1. Load all ItemData assets ───────────────────────────────────
            var assetsByFilename = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in AssetDatabase.FindAssets("t:ItemData"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
                if (item == null) continue;
                string filename = Path.GetFileName(assetPath);
                if (!assetsByFilename.ContainsKey(filename))
                    assetsByFilename[filename] = item;
            }

            // ── 2. Parse existing CSV into row dictionaries ───────────────────
            string[] rawLines = csvAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (rawLines.Length == 0)
                return string.Empty;

            string[] existingHeaders = ParseCsvRow(rawLines[0]);

            bool hasCanStack       = ContainsColumn(existingHeaders, "can_stack");
            bool hasCanBeStackedOn = ContainsColumn(existingHeaders, "can_be_stacked_on");
            if (!hasCanStack)       colsAdded++;
            if (!hasCanBeStackedOn) colsAdded++;

            // Parse data rows as header→value dictionaries.
            var rows = new List<Dictionary<string, string>>();
            for (int i = 1; i < rawLines.Length; i++)
            {
                string line = rawLines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] cols = ParseCsvRow(line);
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int h = 0; h < existingHeaders.Length; h++)
                    dict[existingHeaders[h]] = h < cols.Length ? cols[h] : string.Empty;
                rows.Add(dict);
            }

            // ── 3. Update managed columns in existing rows ────────────────────
            var coveredFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                string assetFile = GetField(row, "asset_file");
                if (string.IsNullOrEmpty(assetFile)) continue;
                coveredFiles.Add(assetFile);

                if (!assetsByFilename.TryGetValue(assetFile, out ItemData item)) continue;

                string newCanStack = item.canStack ? "TRUE" : "FALSE";
                string newCBSO    = item.canBeStackedOn ? "TRUE" : "FALSE";

                bool changed = false;
                if (GetField(row, "can_stack") != newCanStack)
                { row["can_stack"] = newCanStack; changed = true; }
                if (GetField(row, "can_be_stacked_on") != newCBSO)
                { row["can_be_stacked_on"] = newCBSO; changed = true; }
                if (changed) rowsUpdated++;
            }

            // ── 4. Append rows for assets not in the CSV ──────────────────────
            int nextNum = rows.Count + 1;
            foreach (var kvp in assetsByFilename)
            {
                if (coveredFiles.Contains(kvp.Key)) continue;
                ItemData item = kvp.Value;

                string iconFile = string.Empty;
                if (item.icon != null)
                    iconFile = Path.GetFileName(AssetDatabase.GetAssetPath(item.icon));

                var newRow = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["#"]               = nextNum.ToString(),
                    ["itemId"]          = item.itemId   ?? string.Empty,
                    ["itemName"]        = item.itemName ?? string.Empty,
                    ["category"]        = item.category    ?? string.Empty,
                    ["subcategory"]     = item.subcategory ?? string.Empty,
                    ["cost"]            = item.cost.ToString(),
                    ["can_stack"]       = item.canStack       ? "TRUE" : "FALSE",
                    ["can_be_stacked_on"] = item.canBeStackedOn ? "TRUE" : "FALSE",
                    ["icon"]            = item.icon   != null ? "TRUE" : "FALSE",
                    ["prefab"]          = item.prefab  != null ? "TRUE" : "FALSE",
                    ["asset_file"]      = kvp.Key,
                    ["icon_file"]       = iconFile,
                    ["notes"]           = string.Empty,
                };
                rows.Add(newRow);
                nextNum++;
                rowsAppended++;
            }

            // ── 5. Build output column list ───────────────────────────────────
            // Start with canonical order, then append any extra columns from the source CSV.
            var outputCols = new List<string>(CanonicalColumns);
            foreach (string h in existingHeaders)
            {
                if (!IsKnownColumn(h))
                    outputCols.Add(h);
            }

            // ── 6. Serialise to CSV text ──────────────────────────────────────
            var sb = new StringBuilder();
            sb.Append(string.Join(",", outputCols));
            sb.Append("\r\n");

            foreach (var row in rows)
            {
                var fields = new string[outputCols.Count];
                for (int i = 0; i < outputCols.Count; i++)
                    fields[i] = EscapeCsvField(GetField(row, outputCols[i]));
                sb.Append(string.Join(",", fields));
                sb.Append("\r\n");
            }

            string result = sb.ToString();

            // Populate preview (trim to first 200 lines so the UI stays responsive).
            string[] allPreviewLines = result.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int cap = Mathf.Min(allPreviewLines.Length, 200);
            for (int i = 0; i < cap; i++)
                previewLines.Add(allPreviewLines[i]);
            if (allPreviewLines.Length > cap)
                previewLines.Add($"… ({allPreviewLines.Length - cap} more rows not shown)");

            statColsAdded    = colsAdded;
            statRowsUpdated  = rowsUpdated;
            statRowsAppended = rowsAppended;

            return result;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool ContainsColumn(string[] headers, string name)
        {
            foreach (string h in headers)
                if (h.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static bool IsKnownColumn(string name)
        {
            foreach (string k in CanonicalColumns)
                if (k.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static string GetField(Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out string v) ? v : string.Empty;
        }

        /// <summary>Wraps a field in double-quotes if it contains a comma, quote, or newline.</summary>
        private static string EscapeCsvField(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        /// <summary>Splits a single CSV row, respecting quoted fields that may contain commas.</summary>
        private static string[] ParseCsvRow(string line)
        {
            var fields  = new List<string>();
            bool inQ    = false;
            var current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQ && i + 1 < line.Length && line[i + 1] == '"')
                    { current.Append('"'); i++; }
                    else
                        inQ = !inQ;
                }
                else if (c == ',' && !inQ)
                { fields.Add(current.ToString().Trim()); current.Clear(); }
                else
                    current.Append(c);
            }

            fields.Add(current.ToString().Trim());
            return fields.ToArray();
        }
    }
}
