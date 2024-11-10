using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    
    [RequireComponent(typeof(Rigidbody))]
    public class DestructibleObject : MonoBehaviour
    {
        
        [Tooltip("The delay (in seconds) after colliding with the object until it becomes frozen again.")]
        [SerializeField] private float delayToFreeze = 10;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool carHasCollided;

        private Collider collider => GetComponent<Collider>();
        private Rigidbody rigidbody => GetComponent<Rigidbody>();

        public bool CarHasCollided => carHasCollided;
        public float DelayToFreeze => delayToFreeze;

        private void OnEnable()
        {
            DisableCollision();
        }

        private void OnCollisionEnter(Collision collision)
        {
            carHasCollided = true;
        }

        public void DisableCollision()
        {
            collider.enabled = false;
            rigidbody.isKinematic = true;
        }

        public void EnableCollision()
        {
            //reset
            carHasCollided = false;
            
            collider.enabled = true;
            rigidbody.isKinematic = false;
        }
        
    }
}
