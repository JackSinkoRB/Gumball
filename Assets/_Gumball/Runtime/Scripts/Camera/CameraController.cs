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

        private void SetPositionAndRotation()
        {
            if (CurrentState == null)
                return;

            CameraTransition currentTransition = GetCurrentTransition();
            if (currentTransition == null)
            {
                var (position, rotation) = CurrentState.Calculate();
                transform.position = position;
                transform.rotation = rotation;
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
