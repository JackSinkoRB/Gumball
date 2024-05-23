using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DrivingCameraState : CameraState
    {

        [SerializeField] private Transform fakeCamera;
        [SerializeField] private Transform fakeRotationPivot;
        
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float rotationLerpSpeed = 6;
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float rotationLerpSpeedToZero = 2;
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float heightLerpSpeed = 9;
        [SerializeField] private Transform rotationPivot;
        
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
                fakeCamera.transform.position = target.position.SetY(heightInterpolated) + offset.SetY(0);
            }
            else
            {
                Vector3 offsetLocalised = target.TransformPoint(offset);
                fakeCamera.transform.position = offsetLocalised;
            }

            if (!offsetIsLocalised)
            {
                //rotate the pivot around the car with some interpolation:
                //get the angle between the players velocity forward and pivot forward in the XZ plane
                // - if velocity is close to 0, use the players forward
                const float velocityTolerance = 0.5f;
                bool isMoving = carRigidbody.velocity.sqrMagnitude > velocityTolerance;
                Vector3 targetDirection = isMoving ? carRigidbody.velocity.normalized : target.forward;
                float speed = isMoving ? rotationLerpSpeed : rotationLerpSpeedToZero;
                float angleForDesiredRotation = Vector2.SignedAngle(fakeRotationPivot.forward.FlattenAsVector2(), targetDirection.FlattenAsVector2());
                fakeRotationPivot.RotateAround(pivotPoint, Vector3.up, -angleForDesiredRotation * Time.deltaTime * speed);
            }
            
            fakeRotationPivot.LookAt(pivotPoint);
            
            TransformOperation[] operations = new TransformOperation[2];
            operations[0] = new TransformOperation(controller.transform, fakeCamera.transform.position, fakeCamera.transform.rotation);
            operations[1] = new TransformOperation(rotationPivot, fakeRotationPivot.position, fakeRotationPivot.rotation);
            
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
