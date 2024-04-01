using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelColliderData : MonoBehaviour
    {

        [SerializeField, ReadOnly] private bool isGrounded;
        [SerializeField, ReadOnly] private float motorTorque;
        [SerializeField, ReadOnly] private float brakeTorque;
        [SerializeField, ReadOnly] private float rpm;
        [SerializeField, ReadOnly] private float rotationSpeed;
        [SerializeField, ReadOnly] private float steerAngle;
        
        [Header("Friction")]
        [SerializeField, ReadOnly] private float forwardSlip;
        [SerializeField, ReadOnly] private float sidewaysSlip;
        [Tooltip("If the sideways slip is above the sideways extemum value, there will be sliding.")]
        [SerializeField, ReadOnly] private bool isSlidingSideways;
        [Tooltip("If the forward slip is above the forward extemum value, there will be sliding.")]
        [SerializeField, ReadOnly] private bool isSlidingForward;

        private WheelCollider wheelCollider => GetComponent<WheelCollider>();

        /// <summary>
        /// Understeer (front wheels) or oversteer (rear wheels).
        /// </summary>
        public bool IsSlidingSideways => isSlidingSideways;
        
        /// <summary>
        /// Loss of traction (wheels spinning without gripping).
        /// </summary>
        public bool IsSlidingForwards => isSlidingForward;

        private void LateUpdate()
        {
            isGrounded = wheelCollider.isGrounded;
            motorTorque = wheelCollider.motorTorque;
            brakeTorque = wheelCollider.brakeTorque;
            rpm = wheelCollider.rpm;
            rotationSpeed = wheelCollider.rotationSpeed;
            steerAngle = wheelCollider.steerAngle;

            wheelCollider.GetGroundHit(out WheelHit wheelHit);
            forwardSlip = wheelHit.forwardSlip;
            sidewaysSlip = wheelHit.sidewaysSlip;
            
            isSlidingSideways = Mathf.Abs(wheelHit.sidewaysSlip / wheelCollider.sidewaysFriction.extremumSlip) > 1;
            isSlidingForward = Mathf.Abs(wheelHit.forwardSlip / wheelCollider.forwardFriction.extremumSlip) > 1;
        }

    }
}
