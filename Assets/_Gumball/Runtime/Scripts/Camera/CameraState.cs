using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public abstract class CameraState : MonoBehaviour
    {

        [SerializeField, ReadOnly] protected CameraController controller;
        [SerializeField] protected Transform rotationPivot;
        [SerializeField] protected Transform depthPivot;
        [SerializeField] protected Transform lookAtPivot;
        [Space(5)]
        [SerializeField] protected Transform fakeController;
        [SerializeField] protected Transform fakeRotationPivot;
        [SerializeField] protected Transform fakeDepthPivot;
        [SerializeField] protected Transform fakeLookAtPivot;

        private bool isInitialised;

        protected virtual void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        protected virtual void Initialise()
        {
            isInitialised = true;
        }

        public virtual void OnSetCurrent(CameraController controller)
        {
            this.controller = controller;
        }

        public virtual void UpdateWhenCurrent()
        {
            
        }

        public virtual void OnNoLongerCurrent()
        {
            
        }

        public abstract TransformOperation[] Calculate(bool interpolate = true);
        
        public virtual void Snap()
        {
            TransformOperation[] operations = Calculate(false);
            
            foreach (TransformOperation operation in operations)
                operation.Apply();
        }

    }
}
