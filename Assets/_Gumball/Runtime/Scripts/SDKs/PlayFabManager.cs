using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Facebook.Unity;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using UnityEngine;
using UnityEngine.Networking;

namespace Gumball
{
    public static class PlayFabManager
    {

#if UNITY_EDITOR
        public static bool DisableServerTime;
#endif
        
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
#if UNITY_EDITOR
                if (DisableServerTime)
                    return TimeUtils.CurrentEpochSeconds;
#endif
                
                if (ServerTimeInitialisationStatus != ConnectionStatusType.SUCCESS)
                {
                    Debug.LogWarning("Cannot get server time because it hasn't been retrieved. Using local time.");
                    return TimeUtils.CurrentEpochSeconds;
                }

                return serverTimeOnInitialise + Mathf.CeilToInt(Time.realtimeSinceStartup)
                       - gameTimeOnInitialise //account for the time taken to initialise
                       + TimeUtils.TimeOffsetSeconds; //account for debug offset
            }
        }
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            onSuccessfulConnection = null;
#if UNITY_EDITOR
            DisableServerTime = false;
#endif
            
            ConnectionStatus = ConnectionStatusType.LOADING;
            ServerTimeInitialisationStatus = ConnectionStatusType.LOADING;
        }

        public static IEnumerator Initialise()
        {
            if (ConnectionStatus == ConnectionStatusType.SUCCESS && ServerTimeInitialisationStatus == ConnectionStatusType.SUCCESS)
            {
                Debug.LogWarning("Trying to initialise PlayFab, but it is already connected.");
                yield break;
            }

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
            
            //check for cloud save login
            CheckIfAccountHasChanged();
            GlobalLoggers.PlayFabLogger.Log($"Logging in with {CloudSaveManager.CurrentSaveMethod}");
            if (CloudSaveManager.CurrentSaveMethod == CloudSaveManager.SaveMethod.FACEBOOK)
            {
                LoginWithFacebook();
            }
            else
            {
                //no cloud save
                LoginWithCustomID();
            }
        }

        public static void LoginWithFacebook()
        {
            GlobalLoggers.PlayFabLogger.Log($"Logging in with Facebook.");
            LoginWithFacebookRequest request = new LoginWithFacebookRequest
            { 
                CreateAccount = true, 
                AccessToken = AccessToken.CurrentAccessToken.TokenString
            };
            
            PlayFabClientAPI.LoginWithFacebook(request, OnLoginSuccess, OnLoginFailure);
        }

        public static void LoginWithCustomID()
        {
            GlobalLoggers.PlayFabLogger.Log($"Logging in with custom ID.");
            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true
            };
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }
        
        public static void TryUploadData()
        {
            if (ConnectionStatus != ConnectionStatusType.SUCCESS)
                return;
            
            //only upload players with linked accounts
            if (CloudSaveManager.CurrentSaveMethod != CloudSaveManager.SaveMethod.FACEBOOK)
                return;
            
            //ensure data providers are up to date
            DataProvider.SaveAllAsync(() =>
            {
                GlobalLoggers.SaveDataLogger.Log("Uploading save data to Playfab.");
                UploadFiles(DataManager.AllFilePaths); 
            });
        }
        
        private static void CheckIfAccountHasChanged()
        {
            if (CloudSaveManager.CurrentSaveMethod == CloudSaveManager.SaveMethod.FACEBOOK && !FB.IsLoggedIn)
            {
                Debug.Log("Current save method is Facebook, but player is not logged in. Therefore defaulting to local save method.");
                CloudSaveManager.SetCurrentSaveMethod(CloudSaveManager.SaveMethod.LOCAL);
            }
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
            gameTimeOnInitialise = Mathf.CeilToInt(Time.realtimeSinceStartup);
            
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

        private static void UploadFiles(List<string> filePaths)
        {
            //get file names
            List<string> fileNames = new List<string>();
            foreach (string path in filePaths)
                fileNames.Add(Path.GetFileName(path));

            InitiateFileUploadsRequest initiateRequest = new InitiateFileUploadsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey
                {
                    Id = PlayFabSettings.staticPlayer.EntityId,
                    Type = PlayFabSettings.staticPlayer.EntityType,
                },
                FileNames = fileNames
            };
            
            PlayFabDataAPI.InitiateFileUploads(initiateRequest, async result =>
                {
                    GlobalLoggers.PlayFabLogger.Log("Initiated file uploads with PlayFab.");

                    // Step 2: Upload each file to the returned URLs
                    List<Task> uploadTasks = new List<Task>();
                    for (int i = 0; i < result.UploadDetails.Count; i++)
                    {
                        var uploadDetail = result.UploadDetails[i];
                        string filePath = filePaths[i];
                        uploadTasks.Add(UploadFileToUrl(filePath, uploadDetail.UploadUrl));
                    }

                    // Wait for all files to be uploaded
                    await Task.WhenAll(uploadTasks);

                    // Step 3: Finalize the uploads
                    FinalizeUploads(fileNames);
                },
                error =>
                {
                    Debug.LogError("Failed to initiate file uploads: " + error.GenerateErrorReport());
                });
        }
        
        private static async Task UploadFileToUrl(string filePath, string uploadUrl)
        {
            byte[] fileData = await File.ReadAllBytesAsync(filePath);
            UnityWebRequest request = new UnityWebRequest(uploadUrl, "PUT");
            request.uploadHandler = new UploadHandlerRaw(fileData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/octet-stream");

            // Send the request and wait for it to complete
            UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error uploading file {Path.GetFileName(filePath)}: {request.error}");
            }
            else
            {
                GlobalLoggers.PlayFabLogger.Log($"File {Path.GetFileName(filePath)} uploaded successfully.");
            }

            request.Dispose();
        }

        private static void FinalizeUploads(List<string> fileNames)
        {
            var finalizeRequest = new FinalizeFileUploadsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey
                {
                    Id = PlayFabSettings.staticPlayer.EntityId,
                    Type = PlayFabSettings.staticPlayer.EntityType,
                },
                FileNames = fileNames
            };

            PlayFabDataAPI.FinalizeFileUploads(finalizeRequest, result =>
                {
                    GlobalLoggers.PlayFabLogger.Log("Successfully finalized file uploads.");
                },
                error =>
                {
                    Debug.LogError("Failed to finalize file uploads: " + error.GenerateErrorReport());
                });
        }
    }
}
