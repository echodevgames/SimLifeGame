using SeedyRoots.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Displays the player's coin balance in the HUD.
    /// Subscribes to CurrencyManager.OnBalanceChanged and refreshes the label on every event.
    /// </summary>
    public class CurrencyHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI balanceLabel;
        [SerializeField] private Image currencyIcon; // optional — may be null

        private void Start()
        {
            if (CurrencyManager.Instance == null)
            {
                Debug.LogWarning("[CurrencyHUD] CurrencyManager.Instance is null — cannot subscribe to OnBalanceChanged.", this);
                return;
            }

            CurrencyManager.Instance.OnBalanceChanged += Refresh;
            Refresh(CurrencyManager.Instance.Balance);
        }

        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnBalanceChanged -= Refresh;
        }

        /// <summary>Updates the balance label to reflect the new balance.</summary>
        private void Refresh(int balance)
        {
            if (balanceLabel != null)
                balanceLabel.text = $"${balance}";
        }
    }
}
