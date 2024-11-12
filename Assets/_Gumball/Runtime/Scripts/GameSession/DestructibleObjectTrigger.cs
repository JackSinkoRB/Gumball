using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(BoxCollider))]
    public class DestructibleObjectTrigger : MonoBehaviour
    {

        private DestructibleObject destructibleObject => transform.parent.GetComponent<DestructibleObject>();
        private BoxCollider collider => GetComponent<BoxCollider>();

        private Coroutine disableCoroutine;

        private void OnEnable()
        {
            collider.isTrigger = true;
            
            gameObject.layer = (int)LayersAndTags.Layer.DestructibleObjectTrigger;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            destructibleObject.EnableCollision();
            
            //restart disable timer
            if (disableCoroutine != null)
                StopCoroutine(disableCoroutine);
        }

        private void OnTriggerExit(Collider other)
        {
            //restart disable timer
            if (disableCoroutine != null)
                StopCoroutine(disableCoroutine);
            disableCoroutine = this.PerformAfterDelay(destructibleObject.DelayToFreeze, () => destructibleObject.DisableCollision());
        }
        
    }
}
