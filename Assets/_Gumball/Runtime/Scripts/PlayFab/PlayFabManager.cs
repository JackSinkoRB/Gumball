using System.Collections;
using System.Collections.Generic;
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

        public static ConnectionStatusType ConnectionStatus { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            Login();
        }

        private static void Login()
        {
            Debug.Log("Loading PlayFab.");

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
            Debug.Log($"Logged into PlayFab successfully with ID {result.PlayFabId}.");

            //TODO: load playfab data provider

            ConnectionStatus = ConnectionStatusType.SUCCESS;
        }
    }
}
