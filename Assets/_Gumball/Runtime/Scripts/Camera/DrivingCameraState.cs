using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DrivingCameraState : CameraState
    {

        [Serializable]
        private class MomentumSettings
        {
            [SerializeField] private float depth = 1;
            [SerializeField] private float depthDuration = 1;
            [SerializeField] private Ease depthEase;
            [Space(5)]
            [SerializeField] private float height = 1;
            [SerializeField] private float heightDuration = 1;
            [SerializeField] private Ease heightEase;

            public float Depth => depth;
            public float DepthDuration => depthDuration;
            public Ease DepthEase => depthEase;
            
            public float Height => height;
            public float HeightDuration => heightDuration;
            public Ease HeightEase => heightEase;
        }
        
        [SerializeField] private Transform fakeController;
        [SerializeField] private Transform fakeRotationPivot;
        [SerializeField] private Transform fakeDepthPivot;
        [SerializeField] private Transform fakeLookAtPivot;

        [Header("Rotation")]
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float rotationLerpSpeed = 6;
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float rotationLerpSpeedToZero = 2;
        [SerializeField, ConditionalField(nameof(offsetIsLocalised), true)] private float heightLerpSpeed = 9;
        [SerializeField] private Transform rotationPivot;
        [SerializeField] private Transform depthPivot;
        [SerializeField] private Transform lookAtPivot;

        [Header("Momentum")]
        [SerializeField] private MomentumSettings accelerationStartMomentum;
        [SerializeField] private MomentumSettings accelerationEndMomentum;
        [SerializeField] private MomentumSettings brakingStartMomentum;
        [SerializeField] private MomentumSettings brakingEndMomentum;
        [SerializeField] private MomentumSettings gearChangeStartMomentum;
        [SerializeField] private MomentumSettings gearChangeEndMomentum;
        
        private MomentumSettings currentMomentum;
        private Sequence momentumTween;
        private float desiredDepth;
        private float desiredHeight;
        
        [Header("Offsets")]
        [Tooltip("X = width - Y = height - Z = depth")]
        [SerializeField] private Vector3 offset = new(0, 2, -5);
        [SerializeField] private bool offsetIsLocalised;
        [Tooltip("X = width - Y = height - Z = depth")]
        [SerializeField] private Vector3 lookAtOffset = new(0, 1, 0);
        
        [Header("NOS")]
        [SerializeField] private float nosFov = 75;
        [SerializeField] private float nosFovTweenDuration = 1.5f;
        [SerializeField] private Ease nosFovTweenEase = Ease.OutBack;
        [Space(5)]
        [SerializeField] private CameraShakeInstance nosShake;
        
        private float initialFov;
        private Tween fovTween;

        [Header("Collisions")]
        [SerializeField] private float minCollisionMagnitudeForShake;
        [SerializeField] private float collisionMagnitudeForMaxShake;
        [SerializeField] private CameraShakeInstance collisionMaxShake;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] protected Transform otherTarget;
        
        private Transform target => otherTarget != null ? otherTarget : WarehouseManager.Instance.CurrentCar.transform;
        private Rigidbody carRigidbody => WarehouseManager.Instance.CurrentCar.Rigidbody;
        private Vector3 pivotPoint => target.position + (offsetIsLocalised ? target.right * lookAtOffset.x + target.up * lookAtOffset.y + target.forward * lookAtOffset.z : lookAtOffset);

        protected override void Initialise()
        {
            base.Initialise();
            
            initialFov = Camera.main.fieldOfView;
        }
        
        public override void OnSetCurrent(CameraController controller)
        {
            base.OnSetCurrent(controller);
            
            WarehouseManager.Instance.CurrentCar.onGearChanged += OnGearChange;
            WarehouseManager.Instance.CurrentCar.onCollisionEnter += OnCollisionEnter;
        }

        public override void OnNoLongerCurrent()
        {
            base.OnNoLongerCurrent();
            
            WarehouseManager.Instance.CurrentCar.onGearChanged -= OnGearChange;
            WarehouseManager.Instance.CurrentCar.onCollisionEnter -= OnCollisionEnter;

            //reset FOV in case it was in progress
            fovTween?.Kill();
            Camera.main.fieldOfView = initialFov;
        }

        public override void Snap()
        {
            desiredDepth = 0;
            desiredHeight = 0;
            momentumTween?.Kill();
            currentMomentum = null;
            
            base.Snap();
        }

        public override TransformOperation[] Calculate(bool interpolate = true)
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
                float heightInterpolated = interpolate ? Mathf.Lerp(controller.transform.position.y, heightRelativeToCar, Time.deltaTime * heightLerpSpeed) : heightRelativeToCar;
                fakeController.transform.position = target.position.SetY(heightInterpolated) + offset.SetY(0);
            }
            else
            {
                Vector3 offsetLocalised = target.TransformPoint(offset);
                fakeController.transform.position = offsetLocalised;
            }

            const float velocityTolerance = 1f;
            bool isMoving = carRigidbody.velocity.sqrMagnitude > velocityTolerance;
            Vector3 targetDirection = isMoving ? carRigidbody.velocity.normalized : target.forward;
            if (!offsetIsLocalised)
            {
                //rotate the pivot around the car with some interpolation:
                //get the angle between the players velocity forward and pivot forward in the XZ plane
                // - if velocity is close to 0, use the players forward
                float speed = isMoving ? rotationLerpSpeed : rotationLerpSpeedToZero;
                float angleForDesiredRotation = Vector2.SignedAngle(fakeRotationPivot.forward.FlattenAsVector2(), targetDirection.FlattenAsVector2());
                fakeRotationPivot.RotateAround(pivotPoint, Vector3.up, interpolate ? -angleForDesiredRotation * Time.deltaTime * speed : -angleForDesiredRotation);
            }
            
            fakeLookAtPivot.LookAt(pivotPoint);

            //do depth position
            if ((WarehouseManager.Instance.CurrentCar.IsBraking
                 || WarehouseManager.Instance.CurrentCar.IsHandbrakeEngaged)
                && !WarehouseManager.Instance.CurrentCar.IsStationary)
            {
                TryStartMomentumTween(brakingStartMomentum);
            }
            else if (WarehouseManager.Instance.CurrentCar.IsAccelerating
                     && currentMomentum != gearChangeStartMomentum && (currentMomentum != gearChangeEndMomentum || (!momentumTween.IsActive() || momentumTween.IsComplete())))
            {
                TryStartMomentumTween(accelerationStartMomentum);
            }
            else
            {
                TryStopMomentumTween();
            }

            Vector3 desiredDepthPosition = fakeRotationPivot.position + (targetDirection * desiredDepth) + (Vector3.up * desiredHeight);
            fakeDepthPivot.position = desiredDepthPosition;
            
            TransformOperation[] operations = new TransformOperation[4];
            operations[0] = new TransformOperation(controller.transform, fakeController.transform.position, fakeController.transform.rotation);
            operations[1] = new TransformOperation(rotationPivot, fakeRotationPivot.position, fakeRotationPivot.rotation);
            operations[2] = new TransformOperation(depthPivot, fakeDepthPivot.position, fakeDepthPivot.rotation);
            operations[3] = new TransformOperation(lookAtPivot, fakeLookAtPivot.position, fakeLookAtPivot.rotation);

            return operations;
        }

        public void EnableNos(bool isUsingNos)
        {
            TweenFieldOfView(isUsingNos ? nosFov : initialFov, nosFovTweenDuration, nosFovTweenEase);

            if (isUsingNos)
            {
                nosShake.DoShake();
                nosShake.StartFadeIn();
            }
            else
            {
                nosShake.StartFadeOut();
            }
        }

        private void TweenFieldOfView(float fov, float duration, Ease ease)
        {
            fovTween?.Kill();
            fovTween = DOTween.To(() => Camera.main.fieldOfView, 
                    x => Camera.main.fieldOfView = x, fov, duration)
                .SetEase(ease);
        }
        
        private void OnGearChange(int previousGear, int currentGear)
        {
            if (currentGear > previousGear && WarehouseManager.Instance.CurrentCar.IsAccelerating)
                TryStartMomentumTween(gearChangeStartMomentum, gearChangeEndMomentum);
        }
        
        private void TryStartMomentumTween(MomentumSettings settings, MomentumSettings settingsToUseOnComplete = null)
        {
            bool alreadyPlaying = currentMomentum == settings;
            if (alreadyPlaying)
                return;
            
            currentMomentum = settings;
            
            momentumTween?.Kill();
            momentumTween = DOTween.Sequence()
                .Join(DOTween.To(() => desiredDepth, x => desiredDepth = x, settings.Depth, settings.DepthDuration).SetEase(settings.DepthEase))
                .Join(DOTween.To(() => desiredHeight, x => desiredHeight = x, settings.Height, settings.HeightDuration).SetEase(settings.HeightEase));

            if (settingsToUseOnComplete != null)
                momentumTween.OnComplete(() => TryStartMomentumTween(settingsToUseOnComplete));
        }
        
        private void TryStopMomentumTween()
        {
            if (currentMomentum == accelerationStartMomentum)
                TryStartMomentumTween(accelerationEndMomentum);
            
            if (currentMomentum == brakingStartMomentum)
                TryStartMomentumTween(brakingEndMomentum);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            //don't overwrite if already shaking
            if (collisionMaxShake.CurrentState != CameraShakeInstance.State.Inactive)
                return;
            
            float magnitudeSqr = collision.impulse.sqrMagnitude;
            float minMagnitudeSqrRequired = minCollisionMagnitudeForShake * minCollisionMagnitudeForShake;

            if (magnitudeSqr < minMagnitudeSqrRequired)
                return;

            float magnitudeSqrForMaxShake = collisionMagnitudeForMaxShake * collisionMagnitudeForMaxShake;
            if (magnitudeSqrForMaxShake == 0)
                return;
            
            float percent = Mathf.Clamp01(magnitudeSqr / magnitudeSqrForMaxShake);

            if (percent == 0)
                return;
            
            collisionMaxShake.DoShake(percent);
        }

    }
}
