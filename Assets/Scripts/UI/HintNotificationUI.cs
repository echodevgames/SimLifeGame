using System.Collections;
using TMPro;
using UnityEngine;

namespace SeedyRoots.UI
{
    /// <summary>
    /// Singleton HUD notification bar. Call Show(message) from anywhere to display
    /// a brief hint at the top of the screen that auto-fades after displayDuration seconds.
    /// </summary>
    public class HintNotificationUI : MonoBehaviour
    {
        public static HintNotificationUI Instance { get; private set; }

        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI hintLabel;
        [SerializeField] private float displayDuration = 2.5f;
        [SerializeField] private float fadeDuration    = 0.4f;

        private CanvasGroup canvasGroup;
        private Coroutine   hideCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (notificationPanel != null)
            {
                canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }

            HideImmediate();
        }

        /// <summary>Displays message for displayDuration seconds then fades out.</summary>
        public void Show(string message)
        {
            if (notificationPanel == null || hintLabel == null) return;

            hintLabel.text = message;
            notificationPanel.SetActive(true);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);

            hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private void HideImmediate()
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);

            if (canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed        += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    yield return null;
                }
            }

            HideImmediate();
            hideCoroutine = null;
        }
    }
}
