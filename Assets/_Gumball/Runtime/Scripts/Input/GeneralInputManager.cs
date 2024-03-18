using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gumball
{
    public class GeneralInputManager : ActionMapManager
    {

        public InputAction PrimaryContact => GetOrCacheAction("PrimaryContact");
        public InputAction PrimaryPosition => GetOrCacheAction("PrimaryPosition");
        
        protected override string GetActionMapName()
        {
            return "General";
        }

        protected override void OnEnableMap()
        {
            base.OnEnableMap();
            
            PrimaryContact.started += PrimaryContactInput.OnPressed;
            PrimaryContact.canceled += PrimaryContactInput.OnReleased;
            PrimaryContact.performed += PrimaryContactInput.OnPerformed;
        }

        protected override void OnDisableMap()
        {
            base.OnDisableMap();

            PrimaryContact.started -= PrimaryContactInput.OnPressed;
            PrimaryContact.canceled -= PrimaryContactInput.OnReleased;
            PrimaryContact.performed -= PrimaryContactInput.OnPerformed;
        }
        
    }
}
