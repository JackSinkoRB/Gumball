using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public abstract class CameraController : MonoBehaviour
    {
        
        [SerializeField] private CameraState defaultState;

        [Header("Transitions")]
        [SerializeField] private CameraTransition[] transitions;
        
        [SerializeField, ReadOnly] private CameraState currentState;
        [SerializeField, ReadOnly] private CameraState previousState;
        [SerializeField, ReadOnly] private float timeSinceStateChange;

        public CameraState CurrentState
        {
            get
            {
                if (currentState == null)
                    SetState(defaultState);

                return currentState;
            }
        }

        private void LateUpdate()
        {
            timeSinceStateChange += Time.deltaTime;
            
            SetPositionAndRotation();
        }

        //TODO: remove as handled by camera states
        // public void SetTarget(Transform newTarget, bool snap = true)
        // {
        //     if (newTarget == target)
        //         return;
        //     
        //     target = newTarget;
        //     
        //     Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
        //     if (targetRigidbody != null)
        //         targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        //
        //     if (snap && target != null)
        //         CurrentState.SnapToTarget(this, target);
        // }
        
        public CameraTransition GetCurrentTransition()
        {
            foreach (CameraTransition transition in transitions)
            {
                if (transition.From == previousState && transition.To == currentState
                                                     && timeSinceStateChange <= transition.TransitionTime)
                    return transition;
            }

            return null;
        }
        
        public void SetState(CameraState state)
        {
            if (state == currentState)
                return;

            previousState = currentState;
            if (previousState != null)
                previousState.OnNoLongerCurrent();
            
            currentState = state;
            if (currentState != null)
                currentState.OnSetCurrent(this);
            
            timeSinceStateChange = 0;
        }
        
        public void SkipTransition()
        {
            timeSinceStateChange = Mathf.Infinity;
            CurrentState.SnapToTarget();
        }

        
        //TODO: move this to CameraState - all the values should be relative to the state
        [SerializeField] private float rotationLerpSpeed = 6;
        [SerializeField] private float rotationLerpSpeedToZero = 2;
        [SerializeField] private float heightLerpSpeed = 6;
        [SerializeField] private Transform rotationPivot;
        [SerializeField] private Vector3 offset = new(0, 2, -6);
        [SerializeField] private Vector3 lookAtOffset = new(0, 1, 0);

        private void LerpPosition()
        {
            //TODO: move this to CameraState - all the values should be relative to the state
            
            // - should always be looking at the car centre (plus some offset for height)
            // - should always be the same distance away from the car centre
            // - should always have the same height above the car centre
            // - interpolate from current position to desired position (X distance behind car, X distance above car
            // - y (look) rotation is slightly interpolated

            //TODO: move to property in CameraState
            AICar playerCar = WarehouseManager.Instance.CurrentCar;
            Vector3 pivotPoint = playerCar.transform.position + lookAtOffset;
            
            //TODO: cleanup - move to separate method
            //set the position to match the desired offset - but keep height relative to the car
            float heightRelativeToCar = playerCar.transform.TransformPoint(offset).y;
            float heightInterpolated = Mathf.Lerp(transform.position.y, heightRelativeToCar, Time.deltaTime * heightLerpSpeed);
            transform.position = playerCar.transform.position.SetY(heightInterpolated) + offset.SetY(0); //set the position, but interpolate height
            
            //TODO: cleanup - move to separate method
            //rotate the pivot around the car with some interpolation:
            //get the angle between the players velocity forward and pivot forward in the XZ plane
            // - if velocity is close to 0, use the players forward
            const float velocityTolerance = 0.5f;
            bool isMoving = playerCar.Rigidbody.velocity.sqrMagnitude > velocityTolerance;
            Vector3 targetDirection = isMoving ? playerCar.Rigidbody.velocity.normalized : playerCar.transform.forward;
            float speed = isMoving ? rotationLerpSpeed : rotationLerpSpeedToZero;
            float angleForDesiredRotation = Vector2.SignedAngle(rotationPivot.forward.FlattenAsVector2(), targetDirection.FlattenAsVector2());

            rotationPivot.RotateAround(pivotPoint, Vector3.up, -angleForDesiredRotation * Time.deltaTime * speed);
            rotationPivot.LookAt(pivotPoint);
        }
        
        private void SetPositionAndRotation()
        {
            if (CurrentState == null)
                return;

            CameraTransition currentTransition = GetCurrentTransition();
            if (currentTransition == null)
            {
                var (position, rotation) = CurrentState.Calculate();
                
                LerpPosition();
            }
            else
            {
                //blend between the 2 transitions
                var (currentPosition, currentRotation) = CurrentState.Calculate();
                var (previousPosition, previousRotation) = previousState.Calculate();

                float timePercent = Mathf.Clamp01(timeSinceStateChange / currentTransition.TransitionTime);
                float blendPercent = currentTransition.BlendCurve.Evaluate(timePercent);

                Vector3 positionBlended = Vector3.Lerp(previousPosition, currentPosition, blendPercent);
                transform.position = positionBlended;

                Quaternion rotationBlended = Quaternion.Slerp(previousRotation, currentRotation, blendPercent);
                transform.rotation = rotationBlended;
            }
        }
        
    }
}
