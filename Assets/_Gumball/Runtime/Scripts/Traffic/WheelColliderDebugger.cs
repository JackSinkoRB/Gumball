using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelColliderDebugger : MonoBehaviour
    {
#if UNITY_EDITOR

        [SerializeField, ReadOnly] private bool isGrounded;
        [SerializeField, ReadOnly] private float motorTorque;
        [SerializeField, ReadOnly] private float brakeTorque;
        [SerializeField, ReadOnly] private float rpm;
        [SerializeField, ReadOnly] private float rotationSpeed;
        [SerializeField, ReadOnly] private float steerAngle;
        
        [Header("Friction")]
        [SerializeField, ReadOnly] private float forwardSlip;
        [SerializeField, ReadOnly] private float sidewaysSlip;
        [Tooltip("If the sideways slip is above the sideways extemum value, there will be understeer. This can be avoided by increasing the REAR wheel stiffness or lowering the extremum slip.")]
        [SerializeField, ReadOnly] private bool hasUndersteer;

        private WheelCollider wheelCollider => GetComponent<WheelCollider>();
        
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
            
            hasUndersteer = Mathf.Abs(wheelHit.sidewaysSlip / wheelCollider.sidewaysFriction.extremumSlip) > 1;
        }

#endif
    }
}
