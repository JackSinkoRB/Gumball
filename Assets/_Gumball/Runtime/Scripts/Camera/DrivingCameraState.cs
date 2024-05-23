using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DrivingCameraState : CameraState
    {

        [SerializeField] private Transform fakeController;
        [SerializeField] private Transform fakeRotationPivot;
        [SerializeField] private Transform fakeDepthPivot;
        [SerializeField] private Transform fakeCameraPivot;

        [Header("Rotation")]
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float rotationLerpSpeed = 6;
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float rotationLerpSpeedToZero = 2;
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float heightLerpSpeed = 9;
        [SerializeField] private Transform rotationPivot;
        [SerializeField] private Transform depthPivot;
        [SerializeField] private Transform cameraPivot;

        [Header("Depth")]
        [SerializeField] private float brakingDepth = 0.4f;
        [SerializeField] private float brakingHeight = 0.2f;
        [SerializeField] private float brakingDuration = 1.5f;
        [SerializeField] private float heightDelay = 0.1f;
        [SerializeField] private Ease brakingDepthEase;
        [SerializeField] private Ease brakingHeightEase;
        [SerializeField] private float recoverDuration = 0.5f;
        [SerializeField] private Ease recoverDepthEase;
        [SerializeField] private Ease recoverHeightEase;

        private Sequence desiredDepthTween;
        private bool isDoingBrakeAnimation;
        private float desiredDepth;
        private float desiredHeight;
        
        [Header("Offsets")]
        [Tooltip("X = width - Y = height - Z = depth")]
        [SerializeField] private Vector3 offset = new(0, 2, -5);
        [SerializeField] private bool offsetIsLocalised;
        [Tooltip("X = width - Y = height - Z = depth")]
        [SerializeField] private Vector3 lookAtOffset = new(0, 1, 0);
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] protected Transform otherTarget;

        private Transform target => otherTarget != null ? otherTarget : WarehouseManager.Instance.CurrentCar.transform;
        private Rigidbody carRigidbody => WarehouseManager.Instance.CurrentCar.Rigidbody;
        private Vector3 pivotPoint => target.position + lookAtOffset;
        
        public override TransformOperation[] Calculate()
        {
            //TODO: cleanup this method - separate into separate functions
            
            // - should always be looking at the car centre (plus some offset for height)
            // - should always be the same distance away from the car centre
            // - should always have the same height above the car centre
            // - interpolate the rotation around the pivot point
            // - y (look) rotation is slightly interpolated
            
            //get the position to match the desired offset - but keep height relative to the car (with rotation applied)
            if (!offsetIsLocalised)
            {
                float heightRelativeToCar = target.TransformPoint(offset).y;
                float heightInterpolated = Mathf.Lerp(controller.transform.position.y, heightRelativeToCar, Time.deltaTime * heightLerpSpeed);
                fakeController.transform.position = target.position.SetY(heightInterpolated) + offset.SetY(0);
            }
            else
            {
                Vector3 offsetLocalised = target.TransformPoint(offset);
                fakeController.transform.position = offsetLocalised;
            }

            const float velocityTolerance = 0.5f;
            bool isMoving = carRigidbody.velocity.sqrMagnitude > velocityTolerance;
            Vector3 targetDirection = isMoving ? carRigidbody.velocity.normalized : target.forward;
            if (!offsetIsLocalised)
            {
                //rotate the pivot around the car with some interpolation:
                //get the angle between the players velocity forward and pivot forward in the XZ plane
                // - if velocity is close to 0, use the players forward
                float speed = isMoving ? rotationLerpSpeed : rotationLerpSpeedToZero;
                float angleForDesiredRotation = Vector2.SignedAngle(fakeRotationPivot.forward.FlattenAsVector2(), targetDirection.FlattenAsVector2());
                fakeRotationPivot.RotateAround(pivotPoint, Vector3.up, -angleForDesiredRotation * Time.deltaTime * speed);
            }
            
            fakeCameraPivot.LookAt(pivotPoint);
            
            //do depth position
            if (WarehouseManager.Instance.CurrentCar.IsAccelerating
                && !WarehouseManager.Instance.CurrentCar.IsReversing
                && !WarehouseManager.Instance.CurrentCar.IsBraking
                && !WarehouseManager.Instance.CurrentCar.IsHandbrakeEngaged)
            {
                if (isDoingBrakeAnimation)
                {
                    isDoingBrakeAnimation = false;
                    
                    desiredDepthTween?.Kill();
                    desiredDepthTween = DOTween.Sequence()
                        .Join(DOTween.To(() => desiredDepth, x => desiredDepth = x, 0, recoverDuration).SetEase(recoverDepthEase))
                        .Join(DOTween.To(() => desiredHeight, x => desiredHeight = x, 0, recoverDuration).SetEase(recoverHeightEase));
                }
            }
            else
            {
                if (!isDoingBrakeAnimation)
                {
                    isDoingBrakeAnimation = true;
                    
                    desiredDepthTween?.Kill();
                    desiredDepthTween = DOTween.Sequence()
                        .Join(DOTween.To(() => desiredDepth, x => desiredDepth = x, brakingDepth, brakingDuration).SetEase(brakingDepthEase))
                        .Join(DOTween.To(() => desiredHeight, x => desiredHeight = x, brakingHeight, brakingDuration - heightDelay).SetEase(brakingHeightEase).SetDelay(heightDelay));

                }
            }
            
            Vector3 desiredDepthPosition = fakeRotationPivot.position + (targetDirection * desiredDepth) + (Vector3.up * desiredHeight);
            fakeDepthPivot.position = desiredDepthPosition;
            
            TransformOperation[] operations = new TransformOperation[4];
            operations[0] = new TransformOperation(controller.transform, fakeController.transform.position, fakeController.transform.rotation);
            operations[1] = new TransformOperation(rotationPivot, fakeRotationPivot.position, fakeRotationPivot.rotation);
            operations[2] = new TransformOperation(depthPivot, fakeDepthPivot.position, fakeDepthPivot.rotation);
            operations[3] = new TransformOperation(cameraPivot, fakeCameraPivot.position, fakeCameraPivot.rotation);

            return operations;
        }

        public override void Snap()
        {
            //get the position to match the desired offset - but keep height relative to the car (with rotation applied)
            float heightRelativeToCar = target.TransformPoint(offset).y;
            controller.transform.position = target.position.SetY(heightRelativeToCar) + offset.SetY(0);
            
            //rotation pivot:
            const float velocityTolerance = 0.5f;
            bool isMoving = carRigidbody.velocity.sqrMagnitude > velocityTolerance;
            Vector3 targetDirection = isMoving ? carRigidbody.velocity.normalized : target.forward;
            float angleForDesiredRotation = Vector2.SignedAngle(rotationPivot.forward.FlattenAsVector2(), targetDirection.FlattenAsVector2());
            
            rotationPivot.RotateAround(pivotPoint, Vector3.up, -angleForDesiredRotation);
            rotationPivot.LookAt(pivotPoint);
        }
    }
}
