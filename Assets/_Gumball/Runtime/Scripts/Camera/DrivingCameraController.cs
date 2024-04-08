using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    //TODO: take logic and put in a more generic camera controller class that this can derive from
    public class DrivingCameraController : Singleton<DrivingCameraController>
    {

        [Serializable]
        public class CameraTransition
        {
            [SerializeField] private CameraState from;
            [SerializeField] private CameraState to;

            [SerializeField] private float transitionTime = 1f;
            [SerializeField] private AnimationCurve blendCurve;

            public CameraState From => from;
            public CameraState To => to;
            public float TransitionTime => transitionTime;
            public AnimationCurve BlendCurve => blendCurve;
        }
        
        //TODO: this class runs the update on the current state
        //state is a gameobject - all states stay enabled
        //the state calculates it's desired position
        //this class holds transition data (time/ease types) - to/from specific states - else default data
        //if transitioning, calculate both states, and get the middle value between the 2 - the state takes the time percent * the ease curve (gives 0 to 1 value)- while the state transitioning out takes the inverse (1 - X)

        [SerializeField] private CameraState defaultState;
        [SerializeField] private Transform target;

        [Header("States")]
        [SerializeField] private CameraState drivingState;
        [SerializeField] private CameraState introState;

        [SerializeField, ReadOnly] private CameraState currentState;
        [SerializeField, ReadOnly] private CameraState previousState;
        [SerializeField, ReadOnly] private float timeSinceStateChange;

        [Header("Transitions")]
        [SerializeField] private CameraTransition[] transitions;
        
        public CameraState DrivingState => drivingState;
        public CameraState IntroState => introState;

        public CameraState CurrentState
        {
            get
            {
                if (currentState == null)
                    SetState(defaultState);

                return currentState;
            }
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

            if (snap)
                CurrentState.SnapToTarget(this, target);
        }

        private void SetPositionAndRotation()
        {
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
