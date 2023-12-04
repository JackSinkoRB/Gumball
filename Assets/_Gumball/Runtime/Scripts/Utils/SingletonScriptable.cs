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
                    
                    AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(typeof(T).Name);
                    instance = handle.WaitForCompletion();
                    
                    stopwatch.Stop();
                    Debug.LogWarning($"Had to load singleton scriptable {typeof(T).Name} synchronously ({stopwatch.ElapsedMilliseconds}ms)");
                }
                return instance;
            }
        }
        
    }
}
