using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SeedyRoots.Events
{
    /// <summary>
    /// A ScriptableObject-based parameterless event channel.
    /// One asset per named event (e.g. OnMoneyEarned).
    /// Follows Ryan Hipple's Unite 2017 SO event pattern.
    /// </summary>
    [CreateAssetMenu(menuName = "Seedy Roots/Events/Game Event", fileName = "NewGameEvent")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> listeners = new List<GameEventListener>();

        /// <summary>Raises the event, notifying all registered listeners.</summary>
        public void Raise()
        {
            WarnIfNoListeners();

            // Iterate a copy in reverse so mid-raise unregistrations are handled safely.
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        /// <summary>Registers a listener to receive this event.</summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>Unregisters a listener so it no longer receives this event.</summary>
        public void UnregisterListener(GameEventListener listener)
        {
            listeners.Remove(listener);
        }

        [Conditional("UNITY_EDITOR")]
        private void WarnIfNoListeners()
        {
            if (listeners.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"[GameEvent] '{name}' was raised but has no registered listeners.", this);
            }
        }
    }
}
