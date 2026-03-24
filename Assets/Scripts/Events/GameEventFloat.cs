using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SeedyRoots.Events
{
    /// <summary>
    /// ScriptableObject event channel that carries a float payload (e.g. efficiency multipliers, need values).
    /// </summary>
    [CreateAssetMenu(menuName = "Seedy Roots/Events/Game Event (Float)", fileName = "NewGameEventFloat")]
    public class GameEventFloat : ScriptableObject
    {
        private readonly List<GameEventListenerFloat> listeners = new List<GameEventListenerFloat>();

        /// <summary>Raises the event with the given float value.</summary>
        public void Raise(float value)
        {
            WarnIfNoListeners();

            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(value);
            }
        }

        /// <summary>Registers a listener to receive this event.</summary>
        public void RegisterListener(GameEventListenerFloat listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>Unregisters a listener so it no longer receives this event.</summary>
        public void UnregisterListener(GameEventListenerFloat listener)
        {
            listeners.Remove(listener);
        }

        [Conditional("UNITY_EDITOR")]
        private void WarnIfNoListeners()
        {
            if (listeners.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"[GameEventFloat] '{name}' was raised but has no registered listeners.", this);
            }
        }
    }
}
