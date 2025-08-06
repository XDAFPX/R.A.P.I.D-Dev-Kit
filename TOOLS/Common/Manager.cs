using UnityEngine;

// <> denotes this is a generic class
namespace DAFP.GAME.Essential
{
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
                    _instance = FindObjectOfType<T>();
                    Debug.LogWarning($"{nameof(T)} does not exist in the scene( \n CREATING A NEW ONE !!!");
                    // if it's null again create a new object
                    // and attach the generic instance
                    if (_instance == null)
                    {
                        GameObject _obj = new GameObject();
                        _obj.name = typeof(T).Name;
                        var inst = _obj.AddComponent<T>();
                        var man =  inst as Manager<T>;
                            man.Awake();
                    }
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
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}