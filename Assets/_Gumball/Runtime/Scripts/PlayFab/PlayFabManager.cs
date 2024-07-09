using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Gumball
{
    public static class PlayFabManager
    {

        public enum ConnectionStatusType
        {
            LOADING,
            SUCCESS,
            ERROR,
        }

        private static Dictionary<string, string> titleDataCached;
        
        public static ConnectionStatusType ConnectionStatus { get; private set; }
        
        public static void Initialise()
        {
            Login();
        }

        public static T Get<T>(string key, T defaultValue = default)
        {
            if (titleDataCached == null)
                throw new NullReferenceException($"Trying to retrieve title data for {key}, but the title data hasn't been loaded. You should handle cases where the player is offline.");

            if (!titleDataCached.ContainsKey(key))
                return defaultValue;
            
            T deserializedObject = JsonConvert.DeserializeObject<T>(titleDataCached[key]);
            return deserializedObject;
        }

        public static bool HasKey(string key)
        {
            if (titleDataCached == null)
                throw new NullReferenceException("Trying to retrieve title data, but the title data hasn't been loaded.");

            return titleDataCached.ContainsKey(key);
        }

        private static void Login()
        {
            GlobalLoggers.PlayFabLogger.Log("Loading PlayFab.");

            ConnectionStatus = ConnectionStatusType.LOADING;
            
            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }

        private static void OnLoginFailure(PlayFabError error)
        {
            ConnectionStatus = ConnectionStatusType.ERROR;
            
            Debug.LogError($"Could not login to PlayFab: {error.Error} - {error.ErrorMessage}");
        }

        private static void OnLoginSuccess(LoginResult result)
        {
            GlobalLoggers.PlayFabLogger.Log($"Logged into PlayFab successfully with ID {result.PlayFabId}.");

            LoadTitleData();
        }

        private static void LoadTitleData()
        {
            GlobalLoggers.PlayFabLogger.Log($"Loading PlayFab title data.");
            
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
                result =>
                {
                    titleDataCached = result.Data;
                    
                    ConnectionStatus = ConnectionStatusType.SUCCESS;
                    
                    GlobalLoggers.PlayFabLogger.Log($"Loaded {titleDataCached.Count} entries from PlayFab title data.");
                }, error =>
                {
                    ConnectionStatus = ConnectionStatusType.ERROR;
                    
                    Debug.LogError($"Error getting PlayFab title data:\n{error.GenerateErrorReport()}");
                });
        }
    }
}
