using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    /// <summary>
    /// Releases the addressable asset when its destroyed. Do this for correct memory management.
    /// This only works if the object is instantiated through InstantiateAsync. Otherwise you must call Init and pass in the assetreference
    /// </summary>
    public class AddressableReleaseOnDestroy : MonoBehaviour
    {
        private bool manualInit;
        private AsyncOperationHandle handle;

        public void Init(AsyncOperationHandle handle)
        {
            manualInit = true;
            this.handle = handle;
        }

        private void OnDestroy()
        {
            if (manualInit)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            else
            {
                Addressables.ReleaseInstance(gameObject);
            }
        }
    }
}
