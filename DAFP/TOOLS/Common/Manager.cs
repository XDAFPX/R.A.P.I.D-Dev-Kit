using System;
using UnityEngine;

// <> denotes this is a generic class
namespace DAFP.TOOLS.Common
{
    [RequireComponent(typeof(GODManager))]
    public class Manager<T> : MonoBehaviour where T : Component
    {
        // create a private reference to T instance
        private static T _instance;
    
        public static T Singleton
        {
            get
            {
                // if instance is null
                if (_instance == null)
                {
                    // find the generic instance
                    _instance = FindFirstObjectByType<T>();
                    Debug.LogWarning($"{nameof(T)} does not exist in the scene");
                    // if it's null again create a new object
                    // and attach the generic instance
                }
                return _instance;
            }
        }
    
        public virtual void Awake()
        {
            // create the instance
            if (_instance == null)
            {
                _instance = this as T;
               
            }
        }
    }
}