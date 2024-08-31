using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
                    if ((UnityThread.allowsAPI && !Application.isPlaying) || SingletonScriptableHelper.LazyLoadingEnabled)
                    {
                        LoadInstanceSync();
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
        public static void LoadInstanceSync()
        {
            if (IsLoading)
                return;

            if (!UnityThread.allowsAPI && !Application.isPlaying && !Application.isBatchMode) //don't use editor application on CI/CD runners
            {
                //run on the editor thread
                EditorApplication.update -= LoadInstance;
                EditorApplication.update += LoadInstance;
            }
            else
            {
                LoadInstance();
            }

            void LoadInstance()
            {
                handle = Addressables.LoadAssetAsync<T>(typeof(T).Name);
                instance = handle.WaitForCompletion();
                instance.OnInstanceLoaded();

                Debug.LogWarning($"Had to load singleton scriptable {typeof(T).Name} synchronously.");
            }
        }
#endif

        public static IEnumerator LoadInstanceAsync()
        {
            if (IsLoading)
                yield return handle;
            else
            {
                handle = Addressables.LoadAssetAsync<T>(typeof(T).Name);
                yield return handle;
            }

            instance = handle.Result;
            instance.OnInstanceLoaded();
        }

        protected virtual void OnInstanceLoaded()
        {
            
        }
        
    }
}