using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Camera))]
    public class UICamera : MonoBehaviour
    {

        private Camera camera => GetComponent<Camera>();
        
        private void LateUpdate()
        {
            camera.fieldOfView = Camera.main.fieldOfView;
        }
        
    }
}
