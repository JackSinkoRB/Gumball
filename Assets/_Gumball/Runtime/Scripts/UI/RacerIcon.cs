using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class RacerIcon : MonoBehaviour
    {

        [Tooltip("A collection of icons to choose from if the racer doesn't have an icon assigned.")]
        [SerializeField] private Sprite[] defaultIcons;
        [SerializeField] private Image iconImage;

        public Sprite CurrentIcon => iconImage.sprite;
        
        public void SetIcon(Sprite icon)
        {
            if (icon == null)
            {
                iconImage.sprite = defaultIcons.GetRandom();
                return;
            }
            
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
