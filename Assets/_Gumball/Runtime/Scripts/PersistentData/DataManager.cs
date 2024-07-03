using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class DataManager
    {

        //construct providers here:
        public static JsonDataProvider Settings { get; private set; } = new("Settings");
        public static JsonDataProvider Cars { get; private set; } = new("Cars");
        public static JsonDataProvider Avatar { get; private set; } = new("Avatar");
        public static JsonDataProvider GameSessions { get; private set; } = new("GameSessions");
        public static JsonDataProvider Warehouse { get; private set; } = new("Warehouse");
        public static JsonDataProvider Player { get; private set; } = new("Player");

        /// <summary>
        /// Enable or disable whether it reads from the test providers, or the real providers.
        /// </summary>
        public static void EnableTestProviders(bool enableTestProviders)
        {
            Settings = new JsonDataProvider(enableTestProviders ? "Settings_Tests" : "Settings");
            Cars = new JsonDataProvider(enableTestProviders ? "Cars_Tests" : "Cars");
            Avatar = new JsonDataProvider(enableTestProviders ? "Avatar_Tests" : "Avatar");
            GameSessions = new JsonDataProvider(enableTestProviders ? "GameSessions_Tests" : "GameSessions");
            Warehouse = new JsonDataProvider(enableTestProviders ? "Warehouse_Tests" : "Warehouse");
            Player = new JsonDataProvider(enableTestProviders ? "Player_Tests" : "Player");
        }
        
        /// <summary>
        /// Force loads all the data from the sources into the providers asynchronously.
        /// </summary>
        /// <returns>A coroutine that completes when all the data has loaded.</returns>
        public static IEnumerator LoadAllAsync(Action onComplete = null)
        {
            GlobalLoggers.SaveDataLogger.Log("Loading all providers from source (asynchronously).");
            //add any data providers here, and then finally wait for all to be complete
            Settings.LoadFromSourceAsync();
            Cars.LoadFromSourceAsync();
            Avatar.LoadFromSourceAsync();
            GameSessions.LoadFromSourceAsync();
            Warehouse.LoadFromSourceAsync();
            Player.LoadFromSourceAsync();
            yield return new WaitUntil(() => Settings.IsLoaded
                                             && Cars.IsLoaded
                                             && Avatar.IsLoaded
                                             && GameSessions.IsLoaded
                                             && Warehouse.IsLoaded
                                             && Player.IsLoaded);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Force loads all the data from the sources into the providers synchronously.
        /// </summary>
        public static void LoadAllSync()
        {
            GlobalLoggers.SaveDataLogger.Log("Loading all providers from source (synchronously).");
            Settings.LoadFromSourceSync();
            Cars.LoadFromSourceSync();
            Avatar.LoadFromSourceSync();
            GameSessions.LoadFromSourceSync();
            Warehouse.LoadFromSourceSync();
            Player.LoadFromSourceSync();
        }

        public static void RemoveAllData()
        {
            Settings.RemoveFromSource();
            Cars.RemoveFromSource();
            Avatar.RemoveFromSource();
            GameSessions.RemoveFromSource();
            Warehouse.RemoveFromSource();
            Player.RemoveFromSource();
        }
        
    }
}