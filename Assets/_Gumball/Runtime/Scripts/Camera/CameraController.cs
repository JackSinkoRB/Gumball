using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public abstract class CameraController : MonoBehaviour
    {
        
        [SerializeField] private CameraState defaultState;
        [SerializeField] private Transform target;

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

        public void SetTarget(Transform newTarget, bool snap = true)
        {
            if (newTarget == target)
                return;
            
            target = newTarget;
            
            Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
            if (targetRigidbody != null)
                targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            if (snap && target != null)
                CurrentState.SnapToTarget(this, target);
        }
        
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
            currentState = state;
            timeSinceStateChange = 0;
        }
        
        public void SkipTransition()
        {
            timeSinceStateChange = Mathf.Infinity;
            CurrentState.SnapToTarget(this, target);
        }

        private void SetPositionAndRotation()
        {
            if (target == null)
                return;
            
            CameraTransition currentTransition = GetCurrentTransition();
            if (currentTransition == null)
            {
                var (position, lookAtPosition) = CurrentState.Calculate(this, target);
                transform.position = position;
                transform.LookAt(lookAtPosition);
            }
            else
            {
                //blend between the 2 transitions
                var (currentPosition, currentLookAtPosition) = CurrentState.Calculate(this, target);
                var (previousPosition, previousLookAtPosition) = previousState.Calculate(this, target);

                float timePercent = Mathf.Clamp01(timeSinceStateChange / currentTransition.TransitionTime);
                float blendPercent = currentTransition.BlendCurve.Evaluate(timePercent);

                Vector3 positionBlended = Vector3.Lerp(previousPosition, currentPosition, blendPercent);
                transform.position = positionBlended;

                Vector3 lookAtPositionBlended = Vector3.Lerp(previousLookAtPosition, currentLookAtPosition, blendPercent);
                transform.LookAt(lookAtPositionBlended);
            }
        }
        
    }
}
