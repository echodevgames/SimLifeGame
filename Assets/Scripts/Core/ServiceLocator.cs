using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeedyRoots.Core
{
    /// <summary>
    /// Static service locator for global manager access.
    /// Managers register on Awake and unregister on OnDestroy.
    /// Avoids FindObjectOfType and cross-scene static Instance references.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        /// <summary>Registers a service instance under its type key.</summary>
        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting existing registration for '{type.Name}'.");
            }
            services[type] = service;
        }

        /// <summary>Removes the registration for type T.</summary>
        public static void Unregister<T>() where T : class
        {
            services.Remove(typeof(T));
        }

        /// <summary>
        /// Returns the registered instance of type T.
        /// Logs an error and returns null if the type is not registered.
        /// </summary>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceLocator] No service registered for type '{type.Name}'. Ensure the manager is in the scene and has registered itself.");
            return null;
        }
    }
}
