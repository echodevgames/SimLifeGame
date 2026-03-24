using System.Collections.Generic;
using SeedyRoots.Grid;
using UnityEngine;

namespace SeedyRoots.Player
{
    /// <summary>
    /// Tracks which floor tile the player is standing on using trigger enter/exit events.
    /// Maintains a set of all currently-overlapping tiles and applies hysteresis so that
    /// boundary jitter between two equidistant tiles never causes a visible flicker.
    /// Requires a CharacterController on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerStandingTileTracker : MonoBehaviour
    {
        /// <summary>The tile the player is currently standing on, or null if off the grid.</summary>
        public TileHighlighter CurrentTile { get; private set; }

        private readonly HashSet<TileHighlighter> activeTiles = new HashSet<TileHighlighter>();

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<TileHighlighter>(out TileHighlighter tile))
            {
                activeTiles.Add(tile);
                EvaluateCurrentTile();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<TileHighlighter>(out TileHighlighter tile))
            {
                activeTiles.Remove(tile);
                EvaluateCurrentTile();
            }
        }

        /// <summary>
        /// Picks the closest overlapping tile. Only switches away from the current tile
        /// when the new candidate is strictly closer, preventing boundary flicker.
        /// </summary>
        private void EvaluateCurrentTile()
        {
            TileHighlighter closest = null;
            float closestSqr = float.MaxValue;
            Vector3 pos = transform.position;

            foreach (TileHighlighter tile in activeTiles)
            {
                float dx = pos.x - tile.transform.position.x;
                float dz = pos.z - tile.transform.position.z;
                float sqr = dx * dx + dz * dz;

                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    closest = tile;
                }
            }

            if (closest == CurrentTile)
                return;

            // Hysteresis: if the current tile is still overlapping, only switch when
            // the candidate is strictly closer. Equal distance keeps the current tile.
            if (CurrentTile != null && activeTiles.Contains(CurrentTile))
            {
                float dx = pos.x - CurrentTile.transform.position.x;
                float dz = pos.z - CurrentTile.transform.position.z;
                if (closestSqr >= dx * dx + dz * dz)
                    return;
            }

            CurrentTile?.SetStandingHighlight(false);
            CurrentTile = closest;
            CurrentTile?.SetStandingHighlight(true);
        }
    }
}

