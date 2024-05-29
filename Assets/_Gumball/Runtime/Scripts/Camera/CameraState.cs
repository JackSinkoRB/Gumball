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

        public virtual void OnNoLongerCurrent()
        {
            
        }

        public abstract TransformOperation[] Calculate(bool interpolate = true);
        
        public void Snap()
        {
            TransformOperation[] operations = Calculate(false);
            
            foreach (TransformOperation operation in operations)
                operation.Apply();
        }

    }
}
