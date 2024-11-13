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
        
        private const string cloudSaveDeviceIDKey = "CloudSaveDeviceID";
        
        private static Dictionary<string, string> titleDataCached;
        private static Dictionary<string, UserDataRecord> userDataCached;

        private static long serverTimeOnInitialise;
        private static long gameTimeOnInitialise;

        public static ConnectionStatusType LoginStatus { get; private set; }
        public static ConnectionStatusType ServerTimeInitialisationStatus { get; private set; }
        public static ConnectionStatusType TitleDataInitialisationStatus { get; private set; }
        public static ConnectionStatusType UserDataInitialisationStatus { get; private set; }

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
            
            LoginStatus = ConnectionStatusType.LOADING;
            ServerTimeInitialisationStatus = ConnectionStatusType.LOADING;
            TitleDataInitialisationStatus = ConnectionStatusType.LOADING;
            UserDataInitialisationStatus = ConnectionStatusType.LOADING;
        }

        public static IEnumerator Initialise()
        {
            if (LoginStatus == ConnectionStatusType.SUCCESS && ServerTimeInitialisationStatus == ConnectionStatusType.SUCCESS)
            {
                Debug.LogWarning("Trying to initialise PlayFab, but it is already connected.");
                yield break;
            }

            LoginStatus = ConnectionStatusType.LOADING;
            ServerTimeInitialisationStatus = ConnectionStatusType.LOADING;
            TitleDataInitialisationStatus = ConnectionStatusType.LOADING;
            UserDataInitialisationStatus = ConnectionStatusType.LOADING;
            
            Login();
            yield return new WaitUntil(() => LoginStatus != ConnectionStatusType.LOADING);
            
            LoadUserData();
            yield return new WaitUntil(() => UserDataInitialisationStatus != ConnectionStatusType.LOADING);

            LoadTitleData();
            yield return new WaitUntil(() => TitleDataInitialisationStatus != ConnectionStatusType.LOADING);

            RetrieveServerTime();
            yield return new WaitUntil(() => ServerTimeInitialisationStatus != ConnectionStatusType.LOADING);

            if (LoginStatus == ConnectionStatusType.SUCCESS && TitleDataInitialisationStatus == ConnectionStatusType.SUCCESS && UserDataInitialisationStatus == ConnectionStatusType.SUCCESS && ServerTimeInitialisationStatus == ConnectionStatusType.SUCCESS)
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
                LoginStatus = ConnectionStatusType.ERROR;
                
                Debug.LogError($"Could not login to PlayFab: no internet connection");
                return;
            }
            
            LoginStatus = ConnectionStatusType.LOADING;
            
            //check for cloud save login
            CheckIfSaveMethodHasChanged();
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
            if (LoginStatus != ConnectionStatusType.SUCCESS)
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
        
        private static void CheckIfSaveMethodHasChanged()
        {
            if (CloudSaveManager.CurrentSaveMethod == CloudSaveManager.SaveMethod.FACEBOOK && !FB.IsLoggedIn)
            {
                Debug.Log("Current save method is Facebook, but player is not logged in. Therefore defaulting to local save method.");
                CloudSaveManager.SetCurrentSaveMethod(CloudSaveManager.SaveMethod.LOCAL);
            }
        }

        private static void OnLoginFailure(PlayFabError error)
        {
            LoginStatus = ConnectionStatusType.ERROR;
            
            Debug.LogError($"Could not login to PlayFab: {error.Error} - {error.ErrorMessage}");
        }

        private static void OnLoginSuccess(LoginResult result)
        {
            GlobalLoggers.PlayFabLogger.Log($"Logged into PlayFab successfully with ID {result.PlayFabId}.");
            LoginStatus = ConnectionStatusType.SUCCESS;
        }

        private static void LoadUserData()
        {
            GlobalLoggers.PlayFabLogger.Log($"Loading user data.");
            
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnLoadUserDataSuccess, OnLoadUserDataFailure);
        }

        private static void OnLoadUserDataSuccess(GetUserDataResult result)
        {
            userDataCached = result.Data;

            UserDataInitialisationStatus = ConnectionStatusType.SUCCESS;
            
            GlobalLoggers.PlayFabLogger.Log($"Loaded {userDataCached.Count} entries from user data.");

            CheckToSyncCloudData();
        }

        private static void CheckToSyncCloudData()
        {
            GlobalLoggers.PlayFabLogger.Log($"Checking to sync cloud data...");

            if (CloudSaveManager.CurrentSaveMethod != CloudSaveManager.SaveMethod.FACEBOOK)
                return; //no cloud save
            
            if (userDataCached == null || !userDataCached.ContainsKey(cloudSaveDeviceIDKey))
            {
                GlobalLoggers.PlayFabLogger.Log("No saved device ID found.");
                return; //no data saved on cloud
            }

            //if the saved data 'device ID of save' in Player is different - show prompt
            string savedDeviceID = userDataCached[cloudSaveDeviceIDKey].Value;
            string currentDeviceID = SystemInfo.deviceUniqueIdentifier;

            if (savedDeviceID == currentDeviceID)
            {
                GlobalLoggers.PlayFabLogger.Log("Cloud save data is in sync - no need to update.");
                return;
            }
            
            GlobalLoggers.PlayFabLogger.Log("Cloud save is from a different device. Prompting user to sync when ready.");
                            
            //TODO: wait until in main scene to prompt to download
            // - if confirmed, download the save data and continue with login
            // - if denied, log out from facebook and ensure using local save only
            //PromptUserForCloudSaveDownload();
        }

        private static void OnLoadUserDataFailure(PlayFabError error)
        {
            Debug.LogError("Failed to retrieve user data from PlayFab: " + error.GenerateErrorReport());
            
            UserDataInitialisationStatus = ConnectionStatusType.ERROR;
        }

        private static void UpdateCloudSaveDeviceID()
        {
            UpdateUserDataRequest request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { cloudSaveDeviceIDKey, SystemInfo.deviceUniqueIdentifier }
                }
            };

            PlayFabClientAPI.UpdateUserData(request, 
                result => GlobalLoggers.PlayFabLogger.Log("Device ID saved to PlayFab Player Data."), 
                error => Debug.LogError("Failed to save Device ID: " + error.GenerateErrorReport()));
        }

        private static void LoadTitleData()
        {
            GlobalLoggers.PlayFabLogger.Log($"Loading title data.");
            
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
                OnLoadTitleDataSuccess, OnLoadTitleDataFailure);
        }

        private static void OnLoadTitleDataSuccess(GetTitleDataResult result)
        {
            titleDataCached = result.Data;
                    
            TitleDataInitialisationStatus = ConnectionStatusType.SUCCESS;
                    
            GlobalLoggers.PlayFabLogger.Log($"Loaded {titleDataCached.Count} entries from title data.");
        }

        private static void OnLoadTitleDataFailure(PlayFabError error)
        {
            TitleDataInitialisationStatus = ConnectionStatusType.ERROR;
                    
            Debug.LogError($"Error getting PlayFab title data:\n{error.GenerateErrorReport()}");
        }
        
        private static void RetrieveServerTime()
        {
            if (LoginStatus != ConnectionStatusType.SUCCESS)
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
            
            if (LoginStatus == ConnectionStatusType.SUCCESS && TitleDataInitialisationStatus == ConnectionStatusType.SUCCESS && UserDataInitialisationStatus == ConnectionStatusType.SUCCESS && ServerTimeInitialisationStatus == ConnectionStatusType.SUCCESS)
                onSuccess?.Invoke();
            else
                onFailure?.Invoke();
            
            PanelManager.GetPanel<PlayFabReconnectionPanel>().Hide();
        }

        private static void UploadFiles(List<string> filePaths)
        {
            UpdateCloudSaveDeviceID();
            
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
