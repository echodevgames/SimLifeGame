using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SeedyRoots.Events
{
    /// <summary>
    /// ScriptableObject event channel that carries a string payload (e.g. unlock IDs).
    /// </summary>
    [CreateAssetMenu(menuName = "Seedy Roots/Events/Game Event (String)", fileName = "NewGameEventString")]
    public class GameEventString : ScriptableObject
    {
        private readonly List<GameEventListenerString> listeners = new List<GameEventListenerString>();

        /// <summary>Raises the event with the given string value.</summary>
        public void Raise(string value)
        {
            WarnIfNoListeners();

            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(value);
            }
        }

        /// <summary>Registers a listener to receive this event.</summary>
        public void RegisterListener(GameEventListenerString listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>Unregisters a listener so it no longer receives this event.</summary>
        public void UnregisterListener(GameEventListenerString listener)
        {
            listeners.Remove(listener);
        }

        [Conditional("UNITY_EDITOR")]
        private void WarnIfNoListeners()
        {
            if (listeners.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"[GameEventString] '{name}' was raised but has no registered listeners.", this);
            }
        }
    }
}
