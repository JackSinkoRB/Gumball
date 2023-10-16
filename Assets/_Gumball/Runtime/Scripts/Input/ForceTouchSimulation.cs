using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Gumball
{
    public static class ForceTouchSimulation
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Initialise()
        {
            TouchSimulation.Enable();
        }
    }
}
