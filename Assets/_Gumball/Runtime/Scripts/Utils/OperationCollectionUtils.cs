using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task WaitForCompletion(this IEnumerable<AsyncOperationHandle> handles)
        {
            while (!handles.AreAllComplete())
            {
                await Task.Delay(100);
            }
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
