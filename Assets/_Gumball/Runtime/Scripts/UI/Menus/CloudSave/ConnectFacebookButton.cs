using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(MultiImageButton))]
    public class ConnectFacebookButton : MonoBehaviour
    {

        [SerializeField] private Transform loggedInIcon;

        private MultiImageButton button => GetComponent<MultiImageButton>();
        
        private void OnEnable()
        {
            Refresh();
        }

        public void OnClickButton()
        {
            StartCoroutine(LoginWithFacebookIE());
        }

        private void Refresh()
        {
            button.interactable = FacebookManager.LogInStatus != FacebookManager.Status.SUCCESS;
            loggedInIcon.gameObject.SetActive(FacebookManager.LogInStatus == FacebookManager.Status.SUCCESS);
        }
        
        private IEnumerator LoginWithFacebookIE()
        {
            FacebookManager.Login();
            yield return new WaitUntil(() => FacebookManager.LogInStatus != FacebookManager.Status.NOT_ATTEMPTED && FacebookManager.LogInStatus != FacebookManager.Status.LOADING);

            if (FacebookManager.LogInStatus == FacebookManager.Status.SUCCESS)
                CloudSaveManager.SetCurrentSaveMethod(CloudSaveManager.SaveMethod.FACEBOOK);
            
            Refresh();
        }
        
    }
}
