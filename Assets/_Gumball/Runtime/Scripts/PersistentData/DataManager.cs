using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class DataManager
    {

        //construct providers here:
        public static readonly JsonDataProvider Settings = new("Settings");

        /// <summary>
        /// Force loads all the data from the sources into the providers asynchronously.
        /// </summary>
        /// <returns>A coroutine that completes when all the data has loaded.</returns>
        public static IEnumerator LoadAllAsync(Action onComplete = null)
        {
            GlobalLoggers.SaveDataLogger.Log("Loading all providers from source (asynchronously).");
            //add any data providers here, and then finally wait for all to be complete
            Settings.LoadFromSourceAsync();
            yield return new WaitUntil(() => Settings.IsLoaded);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Force loads all the data from the sources into the providers synchronously.
        /// </summary>
        public static void LoadAllSync()
        {
            GlobalLoggers.SaveDataLogger.Log("Loading all providers from source (synchronously).");
            Settings.LoadFromSourceSync();
        }

        public static void RemoveAllData()
        {
            Settings.RemoveFromSource();
        }
        
    }
}