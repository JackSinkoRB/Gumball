using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public abstract class CameraState : MonoBehaviour
    {

        [SerializeField, ReadOnly] protected CameraController controller;

        public virtual void OnSetCurrent(CameraController controller)
        {
            this.controller = controller;
        }

        public virtual void OnNoLongerCurrent()
        {
            
        }

        public abstract TransformOperation[] Calculate();

        public abstract void Snap();

    }
}
