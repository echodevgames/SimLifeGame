using UnityEngine;
using UnityEngine.Events;

namespace SeedyRoots.Events
{
    /// <summary>
    /// MonoBehaviour that subscribes to a GameEventInt SO and invokes a UnityEvent&lt;int&gt; response when raised.
    /// </summary>
    public class GameEventListenerInt : MonoBehaviour
    {
        [Tooltip("The GameEventInt ScriptableObject to listen to.")]
        public GameEventInt Event;

        [Tooltip("UnityEvent response invoked with the int value when the event is raised.")]
        public UnityEvent<int> Response;

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

        /// <summary>Called by GameEventInt when it is raised.</summary>
        public void OnEventRaised(int value)
        {
            Response?.Invoke(value);
        }
    }
}
