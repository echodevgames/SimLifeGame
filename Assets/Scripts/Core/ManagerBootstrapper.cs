using UnityEngine;

namespace SeedyRoots.Core
{
    /// <summary>
    /// Single entry point for ordered manager initialization.
    /// Place this on the [Managers] root GameObject.
    /// Add new manager references here in dependency order as each phase is built.
    /// </summary>
    public class ManagerBootstrapper : MonoBehaviour
    {
        [Header("Core Managers")]
        [Tooltip("Reference to the GameStateManager component in the scene.")]
        [SerializeField] private GameStateManager gameStateManager;

        // Future managers are added below in dependency order:
        // [SerializeField] private EconomyManager economyManager;
        // [SerializeField] private MissionManager missionManager;

        private void Awake()
        {
            ValidateReferences();
            InitializeManagers();
        }

        private void ValidateReferences()
        {
            if (gameStateManager == null)
            {
                Debug.LogError("[ManagerBootstrapper] GameStateManager reference is null. Assign it in the Inspector.", this);
            }
        }

        private void InitializeManagers()
        {
            // Initialize in strict dependency order.
            gameStateManager?.Initialize();

            // Future managers initialized here:
            // economyManager?.Initialize();
            // missionManager?.Initialize();
        }
    }
}
