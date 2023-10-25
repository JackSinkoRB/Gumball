using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CameraController : Singleton<CameraController>
    {
        
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 1;
        [SerializeField] private float height = 0.1f;
        [SerializeField] private float heightDamping = 2;
        [SerializeField] private float lookAtHeight;
        [SerializeField] private float rotationSnapTime = 0.1f;
        [SerializeField] private float distanceSnapTime;
        [SerializeField] private float distanceMultiplier;
        
        private Rigidbody targetRigidbody;
        private float usedDistance;
        private float desiredRotationAngle;
        private float desiredHeight;
        private float currentRotationAngle;
        private float currentHeight;
        private Quaternion currentRotation;
        private Vector3 desiredPosition;
        private float yVelocity;
        private float zVelocity;

        public Transform Target => target;

        private void LateUpdate()
        {
            SmoothFollow();
        }

        public void SetTarget(Transform newTarget, bool snap = true)
        {
            target = newTarget;
            
            if (targetRigidbody == null)
                targetRigidbody = target.GetComponent<Rigidbody>();
            
            if (targetRigidbody != null)
                targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            if (snap)
                SnapToTarget();
        }

        private void SnapToTarget()
        {
            desiredHeight = target.position.y + height;
            desiredRotationAngle = target.eulerAngles.y;
            desiredPosition = target.position;
            desiredPosition.y = desiredHeight;

            usedDistance = distance + (targetRigidbody.velocity.magnitude * distanceMultiplier);
            desiredPosition += Quaternion.Euler(0, desiredRotationAngle, 0) * new Vector3(0, 0, -usedDistance);
            
            transform.position = desiredPosition;
            transform.LookAt(target.position + new Vector3(0, lookAtHeight, 0));
        }

        private void SmoothFollow()
        {
            if (target == null)
                return;

            desiredHeight = target.position.y + height;
            currentHeight = transform.position.y;

            desiredRotationAngle = target.eulerAngles.y;
            currentRotationAngle = transform.eulerAngles.y;

            currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, desiredRotationAngle, ref yVelocity, rotationSnapTime);

            currentHeight = Mathf.Lerp(currentHeight, desiredHeight, heightDamping * Time.deltaTime);

            desiredPosition = target.position;
            desiredPosition.y = currentHeight;

            usedDistance = Mathf.SmoothDampAngle(usedDistance, distance + (targetRigidbody.velocity.magnitude * distanceMultiplier), ref zVelocity, distanceSnapTime);

            desiredPosition += Quaternion.Euler(0, currentRotationAngle, 0) * new Vector3(0, 0, -usedDistance);

            transform.position = desiredPosition;
            transform.LookAt(target.position + new Vector3(0, lookAtHeight, 0));
        }
        
    }
}
