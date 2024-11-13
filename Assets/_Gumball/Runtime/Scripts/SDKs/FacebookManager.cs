using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

namespace Gumball
{
    public static class FacebookManager
    {

        public enum Status
        {
            NOT_ATTEMPTED,
            LOADING,
            SUCCESS,
            ERROR,
        }
        
        public static Status LogInStatus { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            LogInStatus = Status.NOT_ATTEMPTED;
        }
        
        public static IEnumerator Initialise()
        {
            GlobalLoggers.PlayFabLogger.Log("Initialising Facebook...");
            if (FB.IsInitialized)
            {
                OnInitialised();
            }
            else
            {
                FB.Init(OnInitialised);
                yield return new WaitUntil(() => FB.IsInitialized);
            }
        }
        
        private static void OnInitialised()
        {
            FB.ActivateApp();

            GlobalLoggers.PlayFabLogger.Log($"Facebook initialised! Is logged in? {FB.IsLoggedIn}");
        }

        public static void Login()
        {
            LogInStatus = Status.LOADING;
            
            GlobalLoggers.PlayFabLogger.Log("Logging into Facebook...");

            if (FB.IsLoggedIn)
            {
                GlobalLoggers.PlayFabLogger.Log("User already logged in to Facebook.");
                LogInStatus = Status.SUCCESS;
                return;
            }

            //show login screen
            FB.LogInWithReadPermissions(new[] { "public_profile" }, OnFacebookLoggedIn);
        }
        
        private static void OnFacebookLoggedIn(ILoginResult result)
        {
            if (result != null && string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"Facebook Auth Failed: {result.Error} {result.RawResult}");
                LogInStatus = Status.ERROR;
                return;
            }

            GlobalLoggers.PlayFabLogger.Log($"Facebook Auth Complete! Access Token: {AccessToken.CurrentAccessToken.TokenString}\nLogging into PlayFab...");
                
            PlayFabManager.LoginWithFacebook();
        }

    }
}