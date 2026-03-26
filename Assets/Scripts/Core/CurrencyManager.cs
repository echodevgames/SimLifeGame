using System;
using UnityEngine;

namespace SeedyRoots.Core
{
    /// <summary>
    /// Singleton MonoBehaviour that owns the player's coin balance.
    /// Exposes the economy API consumed by store and HUD scripts.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [SerializeField] private int startingBalance = 0;

        public int Balance { get; private set; }

        /// <summary>Fires with the new balance whenever it changes.</summary>
        public event Action<int> OnBalanceChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[CurrencyManager] Duplicate instance detected — destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Balance = startingBalance;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Adds the given amount to the balance. Amount must be greater than zero.</summary>
        public void Add(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] Add called with invalid amount: {amount}. Balance unchanged.");
                return;
            }

            Balance += amount;
            OnBalanceChanged?.Invoke(Balance);
        }

        /// <summary>Returns true if the current balance is greater than or equal to cost.</summary>
        public bool CanAfford(int cost) => Balance >= cost;

        /// <summary>
        /// Attempts to spend the given cost. Returns false without mutating balance if the player cannot afford it.
        /// Cost must be greater than zero.
        /// </summary>
        public bool TrySpend(int cost)
        {
            if (cost <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] TrySpend called with invalid cost: {cost}. Balance unchanged.");
                return false;
            }

            if (!CanAfford(cost))
                return false;

            Balance -= cost;
            OnBalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
