using System;
using SeedyRoots.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Singleton screen-space panel that asks "Pick up [name]?" before lifting a canBeStackedOn item.
    /// Shown by PlayerInteractionController when the nearest item is a host with nothing stacked on it.
    /// Confirm calls the supplied callback then hides; Cancel just hides.
    /// </summary>
    public class PickupPromptUI : MonoBehaviour
    {
        public static PickupPromptUI Instance { get; private set; }

        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI promptLabel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[PickupPromptUI] Duplicate instance detected — destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (promptPanel == null)    Debug.LogWarning("[PickupPromptUI] promptPanel is not assigned.", this);
            if (promptLabel == null)    Debug.LogWarning("[PickupPromptUI] promptLabel is not assigned.", this);
            if (confirmButton == null)  Debug.LogWarning("[PickupPromptUI] confirmButton is not assigned.", this);
            if (cancelButton == null)   Debug.LogWarning("[PickupPromptUI] cancelButton is not assigned.", this);

            if (promptPanel != null)
                promptPanel.SetActive(false);

            IsOpen = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Shows the prompt labelled with the item's name.
        /// onConfirm is invoked then Hide() is called when the player presses Confirm.
        /// </summary>
        public void Show(GridItem item, Action onConfirm)
        {
            if (promptPanel == null) return;

            string displayName = (item.ItemData != null) ? item.ItemData.itemName : item.name;

            if (promptLabel != null)
                promptLabel.text = $"Pick up {displayName}?";

            confirmButton?.onClick.RemoveAllListeners();
            cancelButton?.onClick.RemoveAllListeners();

            confirmButton?.onClick.AddListener(() =>
            {
                onConfirm?.Invoke();
                Hide();
            });

            cancelButton?.onClick.AddListener(Hide);

            promptPanel.SetActive(true);
            IsOpen = true;
        }

        /// <summary>Hides the panel and clears all button listeners.</summary>
        public void Hide()
        {
            confirmButton?.onClick.RemoveAllListeners();
            cancelButton?.onClick.RemoveAllListeners();

            if (promptPanel != null)
                promptPanel.SetActive(false);

            IsOpen = false;
        }
    }
}
