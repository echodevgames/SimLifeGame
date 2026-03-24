using UnityEngine;
using UnityEngine.Events;

namespace SeedyRoots.Events
{
    /// <summary>
    /// MonoBehaviour that subscribes to a GameEvent SO and invokes a UnityEvent response when raised.
    /// Attach to any GameObject that needs to react to a game-wide event.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("The GameEvent ScriptableObject to listen to.")]
        public GameEvent Event;

        [Tooltip("UnityEvent response invoked when the GameEvent is raised.")]
        public UnityEvent Response;

        private void OnEnable()
        {
            if (Event != null)
            {
                Event.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (Event != null)
            {
                Event.UnregisterListener(this);
            }
        }

        /// <summary>Called by the GameEvent when it is raised.</summary>
        public void OnEventRaised()
        {
            Response?.Invoke();
        }
    }
}
