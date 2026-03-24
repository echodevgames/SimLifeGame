using SeedyRoots.Events;
using UnityEngine;

namespace SeedyRoots.Core
{
    public enum GameMode
    {
        Story,
        Free
    }

    public enum ProgressionStage
    {
        BareMinimum = 1,
        GettingBy,
        ScalingUp,
        FullOperation
    }

    /// <summary>
    /// Singleton MonoBehaviour. Single source of truth for global game state.
    /// Tracks the current GameMode and ProgressionStage.
    /// Registers itself with the ServiceLocator on Awake.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Raised when the progression stage changes.")]
        [SerializeField] private GameEvent onStageChanged;

        [Tooltip("Raised when the game mode changes.")]
        [SerializeField] private GameEvent onModeChanged;

        public GameMode CurrentMode { get; private set; } = GameMode.Story;
        public ProgressionStage CurrentStage { get; private set; } = ProgressionStage.BareMinimum;

        private void Awake()
        {
            ServiceLocator.Register<GameStateManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameStateManager>();
        }

        /// <summary>
        /// Initializes the manager to its default state.
        /// Called by ManagerBootstrapper in dependency order.
        /// </summary>
        public void Initialize()
        {
            CurrentMode = GameMode.Story;
            CurrentStage = ProgressionStage.BareMinimum;
            Debug.Log("[GameStateManager] Initialized.");
        }

        /// <summary>Sets the active progression stage and raises OnStageChanged.</summary>
        public void SetStage(ProgressionStage stage)
        {
            CurrentStage = stage;
            onStageChanged?.Raise();
            Debug.Log($"[GameStateManager] Stage changed to: {stage}");
        }

        /// <summary>Sets the active game mode and raises OnModeChanged.</summary>
        public void SetMode(GameMode mode)
        {
            CurrentMode = mode;
            onModeChanged?.Raise();
            Debug.Log($"[GameStateManager] Mode changed to: {mode}");
        }
    }
}
