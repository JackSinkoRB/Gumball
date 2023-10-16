using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gumball
{
    /// <summary>
    /// Holds a reference to an InputAction, and it's value can be overriden with a virtual value.
    /// </summary>
    public abstract class VirtualInputAction
    {
        protected readonly InputAction InputAction;

        protected VirtualInputAction(InputAction inputAction)
        {
            InputAction = inputAction;
        }
    }

    public class VirtualInputActionButton : VirtualInputAction
    {
        public bool IsPressed => InputAction.IsPressed() || isPressedOverride;
        public bool WasPressedThisFrame => InputAction.WasPressedThisFrame() || (IsPressed && Time.frameCount == lastPressedFrame);
        
        private bool isPressedOverride;
        private int lastPressedFrame = Time.frameCount;
            
        public VirtualInputActionButton(InputAction inputAction) : base(inputAction) {}
            
        public void SetPressedOverride(bool pressed)
        {
            isPressedOverride = pressed;
            lastPressedFrame = Time.frameCount;
        }
    }
        
    public class VirtualInputActionFloat : VirtualInputAction
    {
        public float Value => valueOverride ?? InputAction.ReadValue<float>();

        private float? valueOverride;

        public VirtualInputActionFloat(InputAction inputAction) : base(inputAction) {}

        public void SetValueOverride(float? value)
        {
            valueOverride = value;
        }
    }
}
