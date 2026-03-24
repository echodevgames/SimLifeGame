using UnityEngine;
using UnityEngine.Events;

namespace SeedyRoots.Events
{
    /// <summary>
    /// MonoBehaviour that subscribes to a GameEventFloat SO and invokes a UnityEvent&lt;float&gt; response when raised.
    /// </summary>
    public class GameEventListenerFloat : MonoBehaviour
    {
        [Tooltip("The GameEventFloat ScriptableObject to listen to.")]
        public GameEventFloat Event;

        [Tooltip("UnityEvent response invoked with the float value when the event is raised.")]
        public UnityEvent<float> Response;

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

        /// <summary>Called by GameEventFloat when it is raised.</summary>
        public void OnEventRaised(float value)
        {
            Response?.Invoke(value);
        }
    }
}
