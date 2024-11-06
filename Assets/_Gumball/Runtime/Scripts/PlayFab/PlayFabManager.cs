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

        public static event Action onSuccessfulConnection;
        
        private static Dictionary<string, string> titleDataCached;

        private static long serverTimeOnInitialise;
        private static long gameTimeOnInitialise;

        public static ConnectionStatusType ConnectionStatus { get; private set; }
        public static ConnectionStatusType ServerTimeInitialisationStatus { get; private set; }

        /// <summary>
        /// Returns the current time (in epoch seconds) after being synced with the PlayFab server.
        /// </summary>
        public static long CurrentEpochSecondsSynced {

            get {
                if (ServerTimeInitialisationStatus != ConnectionStatusType.SUCCESS)
                {
                    Debug.LogWarning("Cannot get server time because it hasn't been retrieved. Using local time.");
                    return TimeUtils.CurrentEpochSeconds;
                }

                return serverTimeOnInitialise + Mathf.RoundToInt(Time.realtimeSinceStartup)
                       - gameTimeOnInitialise //account for the time taken to initialise
                       + TimeUtils.TimeOffsetSeconds; //account for debug offset
            }
        }
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            onSuccessfulConnection = null;
            
            ConnectionStatus = ConnectionStatusType.LOADING;
            ServerTimeInitialisationStatus = ConnectionStatusType.LOADING;
        }

        public static IEnumerator Initialise()
        {
            ConnectionStatus = ConnectionStatusType.LOADING;
            ServerTimeInitialisationStatus = ConnectionStatusType.LOADING;
            
            Login();
            yield return new WaitUntil(() => ConnectionStatus != ConnectionStatusType.LOADING);
            
            RetrieveServerTime();
            yield return new WaitUntil(() => ServerTimeInitialisationStatus != ConnectionStatusType.LOADING);

            if (ConnectionStatus == ConnectionStatusType.SUCCESS && ServerTimeInitialisationStatus == ConnectionStatusType.SUCCESS)
                onSuccessfulConnection?.Invoke();
        }
        
        /// <summary>
        /// Attempt to initialise again.
        /// </summary>
        public static void AttemptReconnection(Action onSuccess, Action onFailure)
        {
            CoroutineHelper.Instance.StartCoroutine(AttemptReconnectionIE(onSuccess, onFailure));
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

            //check to break quickly if internet is not connected - otherwise the service check may take longer and increase game load time
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ConnectionStatus = ConnectionStatusType.ERROR;
                
                Debug.LogError($"Could not login to PlayFab: no internet connection");
                return;
            }
            
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
        
        private static void RetrieveServerTime()
        {
            if (ConnectionStatus != ConnectionStatusType.SUCCESS)
            {
                ServerTimeInitialisationStatus = ConnectionStatusType.ERROR;
                return;
            }

            PlayFabClientAPI.GetTime(new GetTimeRequest(), OnRetrieveServerTimeSuccess, OnRetrieveServerTimeFailure);
        }

        private static void OnRetrieveServerTimeSuccess(GetTimeResult result)
        {
            serverTimeOnInitialise = new DateTimeOffset(result.Time).ToUnixTimeSeconds();;
            gameTimeOnInitialise = Mathf.RoundToInt(Time.realtimeSinceStartup);
            
            ServerTimeInitialisationStatus = ConnectionStatusType.SUCCESS;
            
            GlobalLoggers.PlayFabLogger.Log($"Successfully retrieved server time.");
        }
        
        private static void OnRetrieveServerTimeFailure(PlayFabError error)
        {
            ServerTimeInitialisationStatus = ConnectionStatusType.ERROR;
            
            Debug.LogWarning($"Failed to get server time: {error.GenerateErrorReport()}");
        }

        private static IEnumerator AttemptReconnectionIE(Action onSuccess, Action onFailure)
        {
            PanelManager.GetPanel<PlayFabReconnectionPanel>().Show();
            
            yield return Initialise();
            
            if (ConnectionStatus == ConnectionStatusType.SUCCESS && ServerTimeInitialisationStatus == ConnectionStatusType.SUCCESS)
                onSuccess?.Invoke();
            else
                onFailure?.Invoke();
            
            PanelManager.GetPanel<PlayFabReconnectionPanel>().Hide();
        }
        
    }
}
