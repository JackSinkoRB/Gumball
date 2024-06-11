using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Collider))]
    public class PlayerResetTrigger : MonoBehaviour
    {

        private Collider collider => GetComponent<Collider>();
        
        private void OnEnable()
        {
            SetupCollider();
        }
        
        private void OnValidate()
        {
            SetupCollider();
        }
        
        private void SetupCollider()
        {
            collider.isTrigger = true;    
            gameObject.layer = (int)LayersAndTags.Layer.PlayerResetTrigger;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerResetManager.Instance.ResetToNearestRandomLane();
        }
        
    }
}
