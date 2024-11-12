using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace Gumball
{
    public static class AddressableUtils
    {

        public static bool DoesAddressExist(string key)
        {
            AsyncOperationHandle<IList<IResourceLocation>> checkExists = Addressables.LoadResourceLocationsAsync(key);
            bool doesExist = checkExists.Status == AsyncOperationStatus.Succeeded && checkExists.Result.Count > 0;
            Addressables.Release(checkExists);
            return doesExist;
        }
        
        /// <remarks>Does not release the handles. This is typically for loading things that will stay in memory (like for ScriptableObject catalogues).</remarks>
        public static IEnumerator LoadAssetsAsync<T>(string label, List<T> list, Action onComplete = null)
        {
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            yield return handle;

            AsyncOperationHandle<T>[] handles = new AsyncOperationHandle<T>[handle.Result.Count];
            for (int index = 0; index < handle.Result.Count; index++)
            {
                IResourceLocation resourceLocation = handle.Result[index];
                handles[index] = Addressables.LoadAssetAsync<T>(resourceLocation);;
                
                handles[index].Completed += h => list.Add(h.Result);
            }

            yield return new WaitUntil(() => handles.AreAllComplete());
            
            Addressables.Release(handle);
            onComplete?.Invoke();
        }

        /// <remarks>Does not release the handles. This is typically for loading things that will stay in memory (like for ScriptableObject catalogues).</remarks>
        public static List<T> LoadAssetsSync<T>(string label)
        {
            List<T> assets = new List<T>();
        
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
            handle.WaitForCompletion();

            foreach (IResourceLocation resourceLocation in handle.Result)
            {
                var locationHandle = Addressables.LoadAssetAsync<T>(resourceLocation);
                locationHandle.WaitForCompletion();
                assets.Add(locationHandle.Result);
            }
        
            Addressables.Release(handle);
            return assets;
        }

    }
}
