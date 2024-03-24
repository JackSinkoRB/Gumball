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

        public static bool AreAllComplete(this IEnumerable<TrackedCoroutine> collection)
        {
            foreach (TrackedCoroutine coroutine in collection)
            {
                if (coroutine.IsPlaying)
                    return false;
            }
            
            return true;
        }
        
        public static async Task WaitForNoNulls<T>(this T[] array, long timeoutSeconds)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            while (array.Any(element => element == null))
            {
                if (stopwatch.Elapsed.Seconds >= timeoutSeconds)
                    throw new TimeoutException("Timeout waiting for array to have no null elements.");
                
                await Task.Delay(100);
            }
        }
        
    }
}
