using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CameraState : MonoBehaviour
    {

        [SerializeField, ReadOnly] protected Transform target;
        
        protected CameraController controller;

        public virtual void OnSetCurrent(CameraController controller)
        {
            this.controller = controller;
        }

        public virtual void OnNoLongerCurrent()
        {
            controller = null;
        }

        public (Vector3, Quaternion) Calculate()
        {
            // desiredRotationAngle = target.eulerAngles.y + horizontalAngleOffset;
            // currentRotationAngle = controller.transform.eulerAngles.y;
            //
            // currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, desiredRotationAngle, ref yVelocity, rotationSnapTime);
            //
            // Vector3 targetPosition = target.position + new Vector3(targetOffsetHorizontal, targetOffsetVertical, 0);
            // desiredPosition = targetPosition;
            //
            // desiredPosition += Quaternion.Euler(0, currentRotationAngle, 0) * new Vector3(0, 0, -distance);
            //
            // Vector3 lookAtPosition = target.position + new Vector3(targetOffsetHorizontal, 0, 0) + new Vector3(0, verticalLookAtOffset, 0);
            // return (desiredPosition, lookAtPosition);

            return (target.position, target.rotation);
        }

        public void SnapToTarget()
        {
            // desiredRotationAngle = target.eulerAngles.y + horizontalAngleOffset;
            //
            // Vector3 targetPosition = target.position + (target.right * targetOffsetHorizontal) + (target.up * targetOffsetVertical);
            // desiredPosition = targetPosition;
            //
            // desiredPosition += Quaternion.Euler(0, desiredRotationAngle, 0) * new Vector3(0, 0, -distance);
            //
            // Vector3 lookAtPosition = target.position + (target.right * targetOffsetHorizontal) + (target.up * verticalLookAtOffset);
            //
            // controller.transform.position = desiredPosition;
            // controller.transform.LookAt(lookAtPosition);

            controller.transform.position = target.position;
            controller.transform.rotation = target.rotation;
        }

    }
}
