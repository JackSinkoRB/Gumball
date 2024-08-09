using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace Gumball
{
#if UNITY_EDITOR
    public static class SingletonScriptableHelper
    {
        /// <summary>
        /// This can be enabled to load the assets on demand.
        /// </summary>
        public static bool LazyLoadingEnabled = false;
    }
#endif
    public class SingletonScriptable<T> : ScriptableObject where T : SingletonScriptable<T>
    {

        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying || SingletonScriptableHelper.LazyLoadingEnabled)
                    {
                        LoadInstanceAsyncEditor();
                        return instance;
                    }
#endif
                    CoroutineHelper.Instance.StartCoroutine(LoadInstanceAsync());
                    throw new NullReferenceException($"Trying to access singleton scriptable {typeof(T).Name}, but it has not loaded yet.");
                }
                
                return instance;
            }
        }

        public static bool HasLoaded => instance != null;
        public static bool IsLoading => handle.IsValid() && !handle.IsDone;
        
        private static AsyncOperationHandle<T> handle;

#if UNITY_EDITOR
        public static void LoadInstanceAsyncEditor()
        {
            handle = Addressables.LoadAssetAsync<T>(typeof(T).Name);
            instance = handle.WaitForCompletion();
            instance.OnInstanceLoaded();
            
            Debug.LogWarning($"Had to load singleton scriptable {typeof(T).Name} synchronously.");
        }
#endif

        public static IEnumerator LoadInstanceAsync()
        {
            Debug.Log("[BUG FIX] Checking to load...");
            if (IsLoading)
            {
                Debug.Log("[BUG FIX] Already loading");
                yield return handle;
            }
            else
            {
                Debug.Log("[BUG FIX] Start loading");
                handle = Addressables.LoadAssetAsync<T>(typeof(T).Name);
                yield return handle;
            }
            
            Debug.Log("[BUG FIX] Completed singleton loading!");
            instance = handle.Result;
            instance.OnInstanceLoaded();
            Debug.Log("[BUG FIX] Called OnInstanceLoaded!");
        }

        protected virtual void OnInstanceLoaded()
        {
            
        }
        
    }
}
