using System.Collections.Generic;
using UnityEngine;

namespace SeedyRoots.Grid
{
    /// <summary>
    /// Singleton that owns the authoritative cell-to-item registry.
    /// Provides cell↔world-space conversion utilities used by all grid-aware systems.
    /// </summary>
    public class GridMap : MonoBehaviour
    {
        public static GridMap Instance { get; private set; }

        /// <summary>Cell size in world units — matches floor tile spacing.</summary>
        public const float CellSize = 2f;

        private readonly Dictionary<Vector2Int, GridItem> grid = new Dictionary<Vector2Int, GridItem>();
        private readonly Dictionary<Vector2Int, TileHighlighter> tiles = new Dictionary<Vector2Int, TileHighlighter>();
        private readonly Dictionary<Vector2Int, GridInteractable> interactables = new Dictionary<Vector2Int, GridInteractable>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GridMap] Duplicate instance detected — destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Converts a world position to the nearest grid coordinate.</summary>
        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x / CellSize);
            int z = Mathf.RoundToInt(worldPosition.z / CellSize);
            return new Vector2Int(x, z);
        }

        /// <summary>Converts a grid coordinate to its world-space centre at y = 0.</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return new Vector3(cell.x * CellSize, 0f, cell.y * CellSize);
        }

        /// <summary>Registers or overwrites a cell's occupant.</summary>
        public void SetItem(Vector2Int cell, GridItem item)
        {
            grid[cell] = item;
        }

        /// <summary>Removes whatever occupies a cell. No-op if the cell is empty.</summary>
        public void ClearCell(Vector2Int cell)
        {
            grid.Remove(cell);
        }

        /// <summary>Returns the item at the given cell, or null if unoccupied.</summary>
        public GridItem GetItem(Vector2Int cell)
        {
            grid.TryGetValue(cell, out GridItem item);
            return item;
        }

        /// <summary>Returns true when the cell has no registered occupant.</summary>
        public bool IsCellFree(Vector2Int cell)
        {
            return !grid.ContainsKey(cell) && !interactables.ContainsKey(cell);
        }

        // ── Tile Highlighter Registry ─────────────────────────────────────

        /// <summary>Called by TileHighlighter on Start to register itself.</summary>
        public void RegisterTile(Vector2Int cell, TileHighlighter tile)
        {
            tiles[cell] = tile;
        }

        /// <summary>Called by TileHighlighter on OnDestroy to unregister itself.</summary>
        public void UnregisterTile(Vector2Int cell)
        {
            tiles.Remove(cell);
        }

        /// <summary>Returns the TileHighlighter at the given cell, or null if none exists.</summary>
        public TileHighlighter GetTile(Vector2Int cell)
        {
            tiles.TryGetValue(cell, out TileHighlighter tile);
            return tile;
        }

        /// <summary>
        /// Returns the TileHighlighter whose world position (XZ only) is closest to worldPos
        /// within maxRadius. Optionally excludes a specific tile. Returns null if none found.
        /// Both worldPos and tile positions are flattened to Y=0 before comparison.
        /// </summary>
        public TileHighlighter GetClosestTile(Vector3 worldPos, float maxRadius, TileHighlighter exclude = null)
        {
            TileHighlighter closest = null;
            float closestSqr = maxRadius * maxRadius;

            foreach (KeyValuePair<Vector2Int, TileHighlighter> kvp in tiles)
            {
                TileHighlighter tile = kvp.Value;
                if (tile == null || tile == exclude)
                    continue;

                Vector3 tilePos = tile.transform.position;
                float dx = worldPos.x - tilePos.x;
                float dz = worldPos.z - tilePos.z;
                float sqr = dx * dx + dz * dz;

                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    closest = tile;
                }
            }

            return closest;
        }

        // ── Interactable Registry ─────────────────────────────────────────

        /// <summary>Called by GridInteractable on Start to register itself.</summary>
        public void RegisterInteractable(Vector2Int cell, GridInteractable interactable)
        {
            interactables[cell] = interactable;
        }

        /// <summary>Called by GridInteractable on OnDestroy to unregister itself.</summary>
        public void UnregisterInteractable(Vector2Int cell)
        {
            interactables.Remove(cell);
        }

        /// <summary>Returns the GridInteractable at the given cell, or null if none exists.</summary>
        public GridInteractable GetInteractable(Vector2Int cell)
        {
            interactables.TryGetValue(cell, out GridInteractable interactable);
            return interactable;
        }
    }
}
