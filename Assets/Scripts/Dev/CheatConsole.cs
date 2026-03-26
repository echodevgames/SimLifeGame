using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using SeedyRoots.Core;
using SeedyRoots.Grid;
using SeedyRoots.Items;
using SeedyRoots.Player;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SeedyRoots.Dev
{
    /// <summary>
    /// Dev-build-only toggle console with a text input field, scrollable echo log,
    /// and built-in cheat commands.
    /// Subscribes directly to the ToggleCheatConsole action in the Dev map of PlayerInputActions,
    /// so no separate Input Actions asset or second PlayerInput is needed.
    /// Toggling open/close deactivates/reactivates the Player map on the scene's PlayerInput
    /// to prevent movement input from leaking through while the console is open.
    /// Compiled only in Editor and Development builds.
    /// </summary>
    public class CheatConsole : MonoBehaviour
    {
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI echoLog;
        [SerializeField] private ScrollRect scrollRect;

        private bool isOpen;
        private InputAction toggleAction;
        private PlayerInput scenePlayerInput;

        private const int DefaultGiveMoneyAmount = 500;

        private void Awake()
        {
            if (consolePanel != null)
                consolePanel.SetActive(false);

            if (echoLog != null)
                echoLog.text = string.Empty;
        }

        private void Start()
        {
            scenePlayerInput = FindFirstObjectByType<PlayerInput>();
            if (scenePlayerInput == null)
            {
                Debug.LogWarning("[CheatConsole] No PlayerInput found in scene.", this);
                return;
            }

            // PlayerInput only auto-enables the default action map.
            // Explicitly enable the Dev map so its bindings are active at all times.
            InputActionMap devMap = scenePlayerInput.actions.FindActionMap("Dev", throwIfNotFound: false);
            if (devMap != null)
                devMap.Enable();
            else
                Debug.LogWarning("[CheatConsole] 'Dev' action map not found in PlayerInputActions.", this);

            toggleAction = scenePlayerInput.actions.FindAction("ToggleCheatConsole", throwIfNotFound: false);
            if (toggleAction != null)
                toggleAction.performed += OnTogglePerformed;
            else
                Debug.LogWarning("[CheatConsole] 'ToggleCheatConsole' action not found in PlayerInputActions.", this);
        }

        private void OnDestroy()
        {
            if (toggleAction != null)
                toggleAction.performed -= OnTogglePerformed;
        }

        private void OnTogglePerformed(InputAction.CallbackContext ctx)
        {
            if (isOpen) Close();
            else Open();
        }

        // ── Open / Close ──────────────────────────────────────────────────────

        private void Open()
        {
            if (consolePanel != null)
                consolePanel.SetActive(true);

            // Disable the Player action map so movement/interaction inputs don't fire.
            scenePlayerInput?.actions.FindActionMap("Player")?.Disable();

            isOpen = true;

            // Wait one frame before activating the input field so the backtick character
            // event from this same frame is not captured and inserted into the field.
            StartCoroutine(ActivateInputNextFrame());
        }

        private IEnumerator ActivateInputNextFrame()
        {
            yield return null;
            if (inputField != null)
            {
                inputField.text = string.Empty;
                inputField.ActivateInputField();
            }
        }

        private void Close()
        {
            if (consolePanel != null)
                consolePanel.SetActive(false);

            // Re-enable the Player action map.
            scenePlayerInput?.actions.FindActionMap("Player")?.Enable();

            if (inputField != null)
                inputField.text = string.Empty;

            isOpen = false;
        }

        // ── Submit ────────────────────────────────────────────────────────────

        /// <summary>Called by the SubmitButton or by pressing Enter in the InputField.</summary>
        public void Submit()
        {
            if (inputField == null) return;

            string raw = inputField.text.Trim();
            if (string.IsNullOrEmpty(raw)) return;

            AppendEcho($"> {raw}");
            ParseAndExecute(raw);

            inputField.text = string.Empty;
            inputField.ActivateInputField();

            ScrollToBottom();
        }

        // ── Command parsing ───────────────────────────────────────────────────

        private void ParseAndExecute(string input)
        {
            string[] parts = input.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string cmd = parts[0].ToLowerInvariant();

            switch (cmd)
            {
                case "givememoney":
                    ExecuteGiveMeMoney(parts);
                    break;

                case "spawnitem":
                    ExecuteSpawnItem(parts);
                    break;

                default:
                    AppendEcho($"Unknown command: {cmd}");
                    break;
            }
        }

        private void ExecuteGiveMeMoney(string[] parts)
        {
            if (CurrencyManager.Instance == null)
            {
                AppendEcho("Error: CurrencyManager not found.");
                return;
            }

            int amount = DefaultGiveMoneyAmount;
            if (parts.Length >= 2 && int.TryParse(parts[1], out int parsed))
                amount = parsed;

            CurrencyManager.Instance.Add(amount);
            AppendEcho($"Added ${amount}. New balance: ${CurrencyManager.Instance.Balance}");
        }

        private void ExecuteSpawnItem(string[] parts)
        {
            if (parts.Length < 2)
            {
                AppendEcho("Usage: spawnitem [itemId]");
                return;
            }

            string itemId = parts[1];

            ItemCatalog catalog = Resources.FindObjectsOfTypeAll<ItemCatalog>().Length > 0
                ? Resources.FindObjectsOfTypeAll<ItemCatalog>()[0]
                : null;

            if (catalog == null)
            {
                AppendEcho("Error: ItemCatalog not found.");
                return;
            }

            ItemData data = catalog.GetItemById(itemId);
            if (data == null) { AppendEcho($"Item not found: {itemId}"); return; }
            if (data.prefab == null) { AppendEcho($"Error: ItemData '{itemId}' has no prefab."); return; }

            PlayerInteractionController controller = FindFirstObjectByType<PlayerInteractionController>();
            if (controller == null) { AppendEcho("Error: PlayerInteractionController not found."); return; }

            GameObject instance = Instantiate(data.prefab, Vector3.zero, Quaternion.identity);
            GridItem gridItem = instance.GetComponent<GridItem>();

            if (gridItem == null)
            {
                AppendEcho($"Error: Prefab for '{itemId}' has no GridItem component.");
                Destroy(instance);
                return;
            }

            controller.ReceiveItem(gridItem);
            AppendEcho($"Spawned '{data.itemName}' and handed to player.");
        }

        // ── Echo helpers ──────────────────────────────────────────────────────

        private void AppendEcho(string line)
        {
            if (echoLog == null) return;
            echoLog.text += $"{line}\n";
        }

        private void ScrollToBottom()
        {
            if (scrollRect == null) return;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
        }
    }
}
#endif
