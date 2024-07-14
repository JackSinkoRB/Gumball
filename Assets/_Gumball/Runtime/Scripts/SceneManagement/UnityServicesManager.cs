using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Gumball
{
    public static class UnityServicesManager
    {

        public static IEnumerator LoadAllServices()
        {
            //can't initialise services like IAP without initialising the core services
            Task initTask = InitialiseUnityServicesAsync();
            yield return new WaitUntil(() => initTask.IsCompleted);
            
            //start loading unity purchasing (async)
            IAPManager.Instance.Initialise();
            
            yield return new WaitUntil(() => IAPManager.Instance.InitialisationStatus != IAPManager.InitialisationStatusType.LOADING);
        }

        private static async Task InitialiseUnityServicesAsync()
        {
            try {
                const string environment = "production";
                InitializationOptions options = new InitializationOptions().SetEnvironmentName(environment);
                await UnityServices.InitializeAsync(options);
            }
            catch (Exception e) {
                Debug.LogError($"Error while initialising Unity Services: {e.Message} - {e.StackTrace}");
            }
        }

    }
}
