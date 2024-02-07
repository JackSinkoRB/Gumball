using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    /// <summary>
    /// Singleton class that will create an instance if one doesn't exist.
    /// </summary>
    /// <remarks>To load from an addressable prefab, set the addressable key to the type name.</remarks>
    /// <remarks>You can add [ExecuteInEditMode] if you need the instance in edit mode (eg. for edit mode tests).</remarks>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        #region STATIC
        /// <summary>
        /// Whether the instance exists at runtime.
        /// </summary>
        public static bool ExistsRuntime { get; private set; }
        
        private static T instance;

        public static T Instance
        {
            get
            {
                AssignInstance();
                return instance;
            }
        }
        
        public static void AssignInstance()
        {
            if (instance != null)
                return;
            
            //try find instance in scene first
            instance = FindObjectOfType<T>();
            if (instance != null)
                return;

            //try find in addressables
            if (AddressableUtils.DoesAddressExist(typeof(T).Name))
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(typeof(T).Name);
                handle.WaitForCompletion();
                instance = Instantiate(handle.Result).GetComponent<T>();
                return;
            }
                    
            //create an instance
            instance = new GameObject(typeof(T).FullName).AddComponent<T>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RuntimeInitialise()
        {
            ExistsRuntime = false;
        }
        #endregion
        
        protected void Awake()
        {
            if (ExistsRuntime)
            {
                //instance already exists, so just remove this new one
                gameObject.SetActive(false); //set inactive so it doesn't trigger colliders etc.
                Destroy(gameObject);
                return;
            }
        
            Initialise();
        }

        private void OnDestroy()
        {
            if (instance == this)
                ExistsRuntime = false;
        }

        private void OnDisable()
        {
            if (instance == this)
                OnInstanceDisabled();
        }

        protected virtual void Initialise()
        {
            instance = this as T;
            
            //persistent singletons always exist
            transform.parent = null; //can't be a child
            DontDestroyOnLoad(gameObject);

            if (Application.isPlaying)
                ExistsRuntime = true;
        }

        /// <summary>
        /// Called when the singleton instance is disabled.
        /// </summary>
        protected virtual void OnInstanceDisabled()
        {
        
        }

    }
}