using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Object = System.Object;

namespace Gumball
{
    public static class ForceTouchSimulation
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialise()
        {
            try
            {
                CoroutineHelper.PerformAfterTrue(() => InputManager.ExistsRuntime, () =>
                {
                    TouchSimulation.Enable();
                    
                    UnityEngine.Object.DontDestroyOnLoad(TouchSimulation.instance.gameObject);

                    //need to set the device as the current device after enabled
                    InputManager.Instance.PlayerInput.SwitchCurrentControlScheme(InputSystem.devices.First(device => device == Touchscreen.current));
                });
            }
            catch (Exception)
            {
                //coroutine helper may not be set up for this scene
            }
        }
    }
}
