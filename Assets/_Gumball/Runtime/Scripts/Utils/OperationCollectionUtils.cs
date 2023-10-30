using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public static class OperationCollectionUtils
    {

        public static bool AreAllComplete(this IEnumerable<AsyncOperationHandle> handles)
        {
            foreach (AsyncOperationHandle handle in handles)
            {
                if (!handle.IsDone)
                    return false;
            }
            
            return true;
        }
        
        public static bool AreAllComplete(this IEnumerable<TrackedCoroutine> collection)
        {
            foreach (TrackedCoroutine coroutine in collection)
            {
                if (coroutine.IsPlaying)
                    return false;
            }
            
            return true;
        }
        
    }
}
