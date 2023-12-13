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
                    instance = handle.WaitForCompletion();
                    stopwatch.Stop();
                    Debug.LogWarning($"Had to load singleton scriptable {typeof(T).Name} synchronously ({stopwatch.ElapsedMilliseconds}ms)");
                    return instance;
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
            handle.Completed += h => instance = h.Result;
            return handle;
        }
    }
}
