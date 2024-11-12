using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gumball
{
    public abstract class ActionMapManager : MonoBehaviour
    {

        protected readonly Dictionary<string, InputAction> actionsCached = new();

        private InputActionMap actionsMapCached;

        private InputActionMap ActionMap =>
            actionsMapCached ??= InputManager.Instance.Controls.FindActionMap(GetActionMapName());
        
        private bool isInitialised;

        public bool IsEnabled { get; private set; }
        
        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        public void Enable(bool enable = true)
        {
            if (ActionMap == null)
            {
                Debug.LogWarning($"Could not find action map with name {GetActionMapName()}");
                return;
            }
            
            if (enable)
            {
                if (IsEnabled)
                    return; //already enabled
                
                ActionMap.Enable();
                OnEnableMap();
            }
            else
            {
                if (!IsEnabled)
                    return; //already disabled
                
                ActionMap.Disable();
                OnDisableMap();
            }
        }

        public void Disable() => Enable(false);

        protected virtual void Initialise()
        {
            isInitialised = true;

            actionsCached.Clear(); //reset so the cache can be updated
        }
        
        protected virtual void OnEnableMap()
        {
            IsEnabled = true;
            GlobalLoggers.InputLogger.Log($"Enabled action map {GetActionMapName()}");
        }

        protected virtual void OnDisableMap()
        {
            IsEnabled = false;
            GlobalLoggers.InputLogger.Log($"Disabled action map {GetActionMapName()}");
        }

        protected InputAction GetOrCacheAction(string action)
        {
            if (!actionsCached.ContainsKey(action))
            {
                actionsCached[action] = InputManager.Instance.Controls.FindAction(action);
                GlobalLoggers.InputLogger.Log($"Updated cache for {action} in {GetActionMapName()}");
            }

            return actionsCached[action];
        }

        protected abstract string GetActionMapName();

    }
}
