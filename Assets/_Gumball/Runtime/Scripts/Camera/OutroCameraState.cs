using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class OutroCameraState : DrivingCameraState
    {
        
        [SerializeField] private float lerpDuration = 3;
        [SerializeField] private AnimationCurve lerpCurve;

        [Tooltip("The distance to look at in front of the car down the road spline.")]
        [SerializeField] private float lookAtDistance = 150;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Vector3 lookAtPoint;
        [SerializeField, ReadOnly] private float timePassed;
        
        private float startVelocityMagnitude;
        
        private float lerpDurationPercent => lerpCurve.Evaluate(Mathf.Clamp01(timePassed / lerpDuration));
        
        public override void OnSetCurrent(CameraController controller)
        {
            this.PerformAtEndOfFrame(() => startVelocityMagnitude = WarehouseManager.Instance.CurrentCar.Rigidbody.velocity.magnitude);
            
            MatchOffsetOfDrivingCamera();

            SetLookAtPoint();

            base.OnSetCurrent(controller);
        }

        public override void UpdateWhenCurrent()
        {
            timePassed += Time.deltaTime;

            base.UpdateWhenCurrent();
        }

        public override Vector3 GetLookAtPoint()
        {
            return lookAtPoint + lookAtOffset;
        }

        public override Vector3 GetPivotPoint()
        {
            return Vector3.Lerp(base.GetPivotPoint(), rotationPivot.transform.position, lerpDurationPercent);
        }

        protected override Vector3 GetPosition(bool interpolate)
        {
            return controller.transform.position + (WarehouseManager.Instance.CurrentCar.Rigidbody.velocity.normalized * (startVelocityMagnitude * (Time.deltaTime * (1 - lerpDurationPercent)))); 
        }

        private void SetLookAtPoint()
        {
            (SplineSample, Chunk)? splineSampleAhead = WarehouseManager.Instance.CurrentCar.GetSplineSampleAhead(lookAtDistance, false);
            if (splineSampleAhead == null)
                throw new NullReferenceException("Look at point exceeds past the map end.");

            lookAtPoint = splineSampleAhead.Value.Item1.position;
        }
        
        private void MatchOffsetOfDrivingCamera()
        {
            DrivingCameraController cameraController = ChunkMapSceneManager.Instance.DrivingCameraController;
            offset = cameraController.CurrentDrivingState.Offset;
        }
        
    }
}