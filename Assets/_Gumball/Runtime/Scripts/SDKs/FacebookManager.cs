using System.Collections;
using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using LoginResult = PlayFab.ClientModels.LoginResult;

namespace Gumball
{
    public static class FacebookManager
    {

        public enum Status
        {
            LOADING,
            SUCCESS,
            ERROR,
        }
        
        public static Status LogInStatus { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            LogInStatus = Status.LOADING;
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

            //TODO: attempt to login
        }
        
        private static void OnInitialised()
        {
            GlobalLoggers.PlayFabLogger.Log($"Facebook initialised! Is logged in? {FB.IsLoggedIn}");
            
            GlobalLoggers.PlayFabLogger.Log("Logging into Facebook...");
            
            // Once Facebook SDK is initialized, if we are logged in, we log out to demonstrate the entire authentication cycle.
            if (FB.IsLoggedIn)
                FB.LogOut();
            
            // We invoke basic login procedure and pass in the callback to process the result
            FB.LogInWithReadPermissions(callback: OnFacebookLoggedIn);
        }

        private static void OnFacebookLoggedIn(ILoginResult result)
        {
            // If result has no errors, it means we have authenticated in Facebook successfully
            if (result == null || string.IsNullOrEmpty(result.Error))
            {
                GlobalLoggers.PlayFabLogger.Log("Facebook Auth Complete! Access Token: " + AccessToken.CurrentAccessToken.TokenString + "\nLogging into PlayFab...");

                /*
                 * We proceed with making a call to PlayFab API. We pass in current Facebook AccessToken and let it create
                 * and account using CreateAccount flag set to true. We also pass the callback for Success and Failure results
                 */
                PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest { CreateAccount = true, AccessToken = AccessToken.CurrentAccessToken.TokenString },
                    OnPlayfabFacebookAuthComplete, OnPlayfabFacebookAuthFailed);
            }
            else
            {
                // If Facebook authentication failed, we stop the cycle with the message
                Debug.LogError($"Facebook Auth Failed: {result.Error}\n{result.RawResult}");
            }
        }

        // When processing both results, we just set the message, explaining what's going on.
        private static void OnPlayfabFacebookAuthComplete(LoginResult result)
        {
            GlobalLoggers.PlayFabLogger.Log("PlayFab Facebook Auth Complete. Session ticket: " + result.SessionTicket);
        }

        private static void OnPlayfabFacebookAuthFailed(PlayFabError error)
        {
            Debug.LogError($"PlayFab Facebook Auth Failed: {error.GenerateErrorReport()}");
        }
        
    }
}