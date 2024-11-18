using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
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

            FacebookManager.onLogin += Refresh;
            FacebookManager.onLogout += Refresh;
        }

        private void OnDisable()
        {
            FacebookManager.onLogin -= Refresh;
            FacebookManager.onLogout -= Refresh;
        }

        public void OnClickButton()
        {
            StartCoroutine(LoginWithFacebookIE());
        }

        private void Refresh()
        {
            button.interactable = !FB.IsLoggedIn;
            loggedInIcon.gameObject.SetActive(FB.IsLoggedIn);
        }
        
        private IEnumerator LoginWithFacebookIE()
        {
            FacebookManager.Login();
            yield return new WaitUntil(() => FB.IsLoggedIn);

            Refresh();
        }
        
    }
}
