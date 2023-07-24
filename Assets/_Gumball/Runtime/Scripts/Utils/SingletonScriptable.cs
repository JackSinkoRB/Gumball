using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
                    AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(typeof(T).Name);
                    instance = handle.WaitForCompletion();
                }
                return instance;
            }
        }
    }
}
