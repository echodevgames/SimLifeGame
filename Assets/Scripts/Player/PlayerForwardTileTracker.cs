using SeedyRoots.Grid;
using UnityEngine;

namespace SeedyRoots.Player
{
    /// <summary>
    /// Tracks and highlights the floor tile directly ahead of the player using a SphereCast.
    /// The cast travels in transform.forward from a point at the tile collider's vertical
    /// centre, returning the first tile trigger it physically intersects.
    /// A frame-count stabilizer prevents flickering during rotation: a candidate must be
    /// detected consistently for <see cref="stableFramesRequired"/> frames before it commits.
    /// </summary>
    [RequireComponent(typeof(PlayerStandingTileTracker))]
    public class PlayerForwardTileTracker : MonoBehaviour
    {
        [Tooltip("Vertical offset from the player's transform origin for the cast origin. " +
                 "Should sit inside the tile trigger height (tile triggers span 0–0.5 in Y).")]
        [SerializeField] private float castOriginHeight = 0.25f;

        [Tooltip("Radius of the SphereCast. Keep narrow (≤0.4) to avoid catching diagonal tiles.")]
        [SerializeField] private float castRadius = 0.35f;

        [Tooltip("Maximum forward distance of the SphereCast. Tiles are 2 units apart, so 2.5 is a safe maximum.")]
        [SerializeField] private float castDistance = 2.5f;

        [Tooltip("How many consecutive frames a candidate must be detected before it becomes the active ForwardTile. " +
                 "Higher values are more stable but slightly less responsive on fast turns.")]
        [SerializeField] private int stableFramesRequired = 3;

        /// <summary>The tile currently highlighted as the forward / interactable tile, or null.</summary>
        public TileHighlighter ForwardTile { get; private set; }

        private PlayerStandingTileTracker standingTracker;
        private readonly RaycastHit[] hitBuffer = new RaycastHit[8];

        private TileHighlighter pendingCandidate;
        private int stableFrameCount;

        private void Awake()
        {
            standingTracker = GetComponent<PlayerStandingTileTracker>();
        }

        private void Update()
        {
            TileHighlighter candidate = CastForwardTile();

            // Track how many consecutive frames this candidate has been seen.
            if (candidate == pendingCandidate)
            {
                stableFrameCount++;
            }
            else
            {
                pendingCandidate = candidate;
                stableFrameCount = 1;
            }

            // Only commit once the candidate is stable — same tile, N frames in a row.
            if (pendingCandidate == ForwardTile || stableFrameCount < stableFramesRequired)
                return;

            ForwardTile?.SetHighlighted(false);
            ForwardTile = pendingCandidate;
            ForwardTile?.SetHighlighted(true);
        }

        private void OnDisable()
        {
            ForwardTile?.SetHighlighted(false);
            ForwardTile = null;
            pendingCandidate = null;
            stableFrameCount = 0;
        }

        /// <summary>
        /// Runs a SphereCast in the player's forward direction and returns the closest
        /// tile trigger hit, excluding the tile the player is currently standing on.
        /// </summary>
        private TileHighlighter CastForwardTile()
        {
            Vector3 origin = transform.position + Vector3.up * castOriginHeight;

            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                castRadius,
                transform.forward,
                hitBuffer,
                castDistance,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide);

            TileHighlighter standing = standingTracker.CurrentTile;
            TileHighlighter best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                // distance == 0 means the cast sphere was already overlapping this collider
                // at the origin — the tile is beside or behind the player, not ahead.
                if (hitBuffer[i].distance <= 0f)
                    continue;

                if (!hitBuffer[i].collider.TryGetComponent<TileHighlighter>(out TileHighlighter tile))
                    continue;

                if (tile == standing)
                    continue;

                if (hitBuffer[i].distance < bestDistance)
                {
                    bestDistance = hitBuffer[i].distance;
                    best = tile;
                }
            }

            return best;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * castOriginHeight;
            Vector3 end = origin + transform.forward * castDistance;

            // Committed forward tile — bright yellow.
            Gizmos.color = ForwardTile != null ? new Color(1f, 0.92f, 0f) : new Color(1f, 0.92f, 0f, 0.3f);
            Gizmos.DrawWireSphere(origin, castRadius);
            Gizmos.DrawWireSphere(end, castRadius);
            Gizmos.DrawLine(origin + transform.up * castRadius, end + transform.up * castRadius);
            Gizmos.DrawLine(origin - transform.up * castRadius, end - transform.up * castRadius);
            Gizmos.DrawLine(origin + transform.right * castRadius, end + transform.right * castRadius);
            Gizmos.DrawLine(origin - transform.right * castRadius, end - transform.right * castRadius);

            // Pending candidate — orange, only drawn while waiting to commit.
            if (pendingCandidate != null && pendingCandidate != ForwardTile)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f);
                Gizmos.DrawWireSphere(pendingCandidate.transform.position + Vector3.up * castOriginHeight, castRadius * 0.6f);
            }
        }
#endif
    }
}

