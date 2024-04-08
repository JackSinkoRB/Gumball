using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CameraState : MonoBehaviour
    {

        [SerializeField] private float targetOffsetHorizontal;
        [SerializeField] private float targetOffsetVertical = 0.35f;
        [SerializeField] private float verticalLookAtOffset;
        [SerializeField] private float horizontalAngleOffset;
        [SerializeField] private float distance = 2.3f;
        [SerializeField] private float rotationSnapTime = 0.15f;
        
        private float desiredRotationAngle;
        private float desiredHeight;
        private float currentRotationAngle;
        private Quaternion currentRotation;
        private Vector3 desiredPosition;
        private float yVelocity;
        private float zVelocity;

        public (Vector3, Vector3) Calculate(DrivingCameraController controller, Transform target)
        {
            desiredRotationAngle = target.eulerAngles.y + horizontalAngleOffset;
            currentRotationAngle = controller.transform.eulerAngles.y;

            currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, desiredRotationAngle, ref yVelocity, rotationSnapTime);

            Vector3 targetPosition = target.position + new Vector3(targetOffsetHorizontal, targetOffsetVertical, 0);
            desiredPosition = targetPosition;
            
            desiredPosition += Quaternion.Euler(0, currentRotationAngle, 0) * new Vector3(0, 0, -distance);
            
            Vector3 lookAtPosition = target.position + new Vector3(targetOffsetHorizontal, 0, 0) + new Vector3(0, verticalLookAtOffset, 0);
            return (desiredPosition, lookAtPosition);
        }

        public void SnapToTarget(DrivingCameraController controller, Transform target)
        {
            desiredRotationAngle = target.eulerAngles.y + horizontalAngleOffset;
            
            Vector3 targetPosition = target.position + (target.right * targetOffsetHorizontal) + (target.up * targetOffsetVertical);
            desiredPosition = targetPosition;
            
            desiredPosition += Quaternion.Euler(0, desiredRotationAngle, 0) * new Vector3(0, 0, -distance);
            
            Vector3 lookAtPosition = target.position + (target.right * targetOffsetHorizontal) + (target.up * verticalLookAtOffset);
            
            controller.transform.position = desiredPosition;
            controller.transform.LookAt(lookAtPosition);
        }

    }
}
