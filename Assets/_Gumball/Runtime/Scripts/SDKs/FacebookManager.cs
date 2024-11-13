using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

namespace Gumball
{
    public static class FacebookManager
    {

        public static event Action onLogin;
        public static event Action onLogout;

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

            if (FB.IsLoggedIn)
            {
                //if for some reason the player is logged into facebook but the cloud save method is not facebook, then set it anyway
                if (CloudSaveManager.CurrentSaveMethod != CloudSaveManager.SaveMethod.FACEBOOK)
                    CloudSaveManager.SetCurrentSaveMethod(CloudSaveManager.SaveMethod.FACEBOOK);
                
                //trigger the onLogin event as the login was automatic
                onLogin?.Invoke();
            }
        }

        public static void Login()
        {
            GlobalLoggers.PlayFabLogger.Log("Logging into Facebook...");

            if (FB.IsLoggedIn)
            {
                GlobalLoggers.PlayFabLogger.Log("User already logged in to Facebook.");
                return;
            }

            //show login screen
            FB.LogInWithReadPermissions(new[] { "public_profile" }, OnFacebookLoggedIn);
        }

        public static void Logout()
        {
            FB.LogOut();
            
            CloudSaveManager.SetCurrentSaveMethod(CloudSaveManager.SaveMethod.LOCAL);

            GlobalLoggers.PlayFabLogger.Log("Logged out of Facebook.");

            onLogout?.Invoke();
        }
        
        private static void OnFacebookLoggedIn(ILoginResult result)
        {
            if (result == null || !string.IsNullOrEmpty(result.Error))
            {
                GlobalLoggers.PlayFabLogger.Log($"Facebook Auth Failed: {(result != null ? result.Error : "")} {(result != null ? result.RawResult : "")}");
                FB.LogOut();
                onLogout?.Invoke();
                return;
            }

            GlobalLoggers.PlayFabLogger.Log($"Facebook Auth Complete! Access Token: {AccessToken.CurrentAccessToken.TokenString}\nLogging into PlayFab...");
            
            CloudSaveManager.SetCurrentSaveMethod(CloudSaveManager.SaveMethod.FACEBOOK);
            
            PlayFabManager.LoginWithFacebook();

            onLogin?.Invoke();
        }

    }
}