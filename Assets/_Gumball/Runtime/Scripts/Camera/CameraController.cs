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

        private void Update()
        {
            timeSinceStateChange += Time.deltaTime;
            
            SetPositionAndRotation();
            
            if (currentState != null)
                currentState.UpdateWhenCurrent();
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
            CurrentState.Snap();
        }
        
        private void SetPositionAndRotation()
        {
            if (CurrentState == null)
                return;

            CameraTransition currentTransition = GetCurrentTransition();
            if (currentTransition == null)
            {
                TransformOperation[] operations = CurrentState.Calculate();

                foreach (TransformOperation operation in operations)
                    operation.Apply();
            }
            else
            {
                //foreach operation in CurrentState, check if there's a corresponding target
                // if yes, blend the 2 positions and rotations
                // if no, just apply the operation
                
                //blend between the 2 transitions
                TransformOperation[] currentOperations = CurrentState.Calculate();
                TransformOperation[] previousOperations = previousState.Calculate();

                float timePercent = Mathf.Clamp01(timeSinceStateChange / currentTransition.TransitionTime);
                float blendPercent = currentTransition.BlendCurve.Evaluate(timePercent);

                foreach (TransformOperation previousOperation in previousOperations)
                {
                    foreach (TransformOperation currentOperation in currentOperations)
                    {
                        if (previousOperation.TryBlend(currentOperation, blendPercent))
                            break;
                    }
                }
            }
        }
        
    }
}
