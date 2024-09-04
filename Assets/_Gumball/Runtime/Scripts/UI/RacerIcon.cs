using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class RacerIcon : MonoBehaviour
    {

        [SerializeField] private Image iconImage;
        
        public void SetIcon(Sprite icon)
        {
            iconImage.sprite = icon;
        }
        
        private void LateUpdate()
        {
            BillboardToCamera();
        }

        private void BillboardToCamera()
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
        
    }
}
