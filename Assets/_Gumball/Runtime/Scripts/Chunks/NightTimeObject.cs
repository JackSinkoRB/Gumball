using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class NightTimeObject : MonoBehaviour
    {
        
        private void OnEnable()
        {
            CheckToDisable();
        }

        private void CheckToDisable()
        {
            if (!GameSessionManager.ExistsRuntime
                || GameSessionManager.Instance.CurrentSession == null)
                return;
            
            if (!GameSessionManager.Instance.CurrentSession.IsNightTime)
                gameObject.SetActive(false);
        }
        
    }
}
