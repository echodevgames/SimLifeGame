using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SeedyRoots.Events
{
    /// <summary>
    /// ScriptableObject event channel that carries an int payload (e.g. money amounts).
    /// </summary>
    [CreateAssetMenu(menuName = "Seedy Roots/Events/Game Event (Int)", fileName = "NewGameEventInt")]
    public class GameEventInt : ScriptableObject
    {
        private readonly List<GameEventListenerInt> listeners = new List<GameEventListenerInt>();

        /// <summary>Raises the event with the given int value.</summary>
        public void Raise(int value)
        {
            WarnIfNoListeners();

            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(value);
            }
        }

        /// <summary>Registers a listener to receive this event.</summary>
        public void RegisterListener(GameEventListenerInt listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>Unregisters a listener so it no longer receives this event.</summary>
        public void UnregisterListener(GameEventListenerInt listener)
        {
            listeners.Remove(listener);
        }

        [Conditional("UNITY_EDITOR")]
        private void WarnIfNoListeners()
        {
            if (listeners.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"[GameEventInt] '{name}' was raised but has no registered listeners.", this);
            }
        }
    }
}
