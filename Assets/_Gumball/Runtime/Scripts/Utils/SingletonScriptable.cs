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
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    LoadInstanceAsync();
                    
#if UNITY_EDITOR
                    if (!Application.isPlaying || SingletonScriptableHelper.LazyLoadingEnabled)
                    {
                        instance = handle.WaitForCompletion();
                        stopwatch.Stop();
                        Debug.LogWarning($"Had to load singleton scriptable {typeof(T).Name} synchronously ({stopwatch.ElapsedMilliseconds}ms)");
                        return instance;
                    }
#endif

                    throw new NullReferenceException($"Trying to access singleton scriptable {typeof(T).Name}, but it has not loaded yet.");
                }
                return instance;
            }
        }

        public static bool HasLoaded => instance != null;
        public static bool IsLoading => handle.IsValid() && !handle.IsDone;
        
        private static AsyncOperationHandle<T> handle;
        
        public static AsyncOperationHandle LoadInstanceAsync()
        {
            if (IsLoading)
                return handle;
            
            handle = Addressables.LoadAssetAsync<T>(typeof(T).Name);
            handle.Completed += h =>
            {
                Debug.Log("[BUG FIX] 1 Handle was loaded");
                instance = h.Result;
                instance.OnInstanceLoaded();
                Debug.Log("[BUG FIX] 2 Instance was initialised");
            };
            return handle;
        }

        protected virtual void OnInstanceLoaded()
        {
            
        }
        
    }
}
