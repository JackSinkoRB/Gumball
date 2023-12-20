using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

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
        
    }
}
