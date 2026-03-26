using UnityEngine;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Central coordinator for the persistent in-game HUD.
    /// Holds serialized references to each status section so they can be
    /// individually shown, hidden, or animated from a single point.
    ///
    /// Current skeleton sections:
    ///   Financial  — currency balance (active)
    ///   Social     — placeholder
    ///   Personal   — placeholder
    ///
    /// Add new section GameObjects here as they are built out in later phases.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("HUD Sections")]
        [SerializeField] private GameObject financialSection;
        [SerializeField] private GameObject socialSection;
        [SerializeField] private GameObject personalSection;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Shows or hides the financial status section.</summary>
        public void SetFinancialVisible(bool visible)
        {
            if (financialSection != null)
                financialSection.SetActive(visible);
        }

        /// <summary>Shows or hides the social status section.</summary>
        public void SetSocialVisible(bool visible)
        {
            if (socialSection != null)
                socialSection.SetActive(visible);
        }

        /// <summary>Shows or hides the personal status section.</summary>
        public void SetPersonalVisible(bool visible)
        {
            if (personalSection != null)
                personalSection.SetActive(visible);
        }
    }
}
