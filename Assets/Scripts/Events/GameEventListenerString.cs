using UnityEngine;
using UnityEngine.Events;

namespace SeedyRoots.Events
{
    /// <summary>
    /// MonoBehaviour that subscribes to a GameEventString SO and invokes a UnityEvent&lt;string&gt; response when raised.
    /// </summary>
    public class GameEventListenerString : MonoBehaviour
    {
        [Tooltip("The GameEventString ScriptableObject to listen to.")]
        public GameEventString Event;

        [Tooltip("UnityEvent response invoked with the string value when the event is raised.")]
        public UnityEvent<string> Response;

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

        /// <summary>Called by GameEventString when it is raised.</summary>
        public void OnEventRaised(string value)
        {
            Response?.Invoke(value);
        }
    }
}
