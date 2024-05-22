using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class NosButton : MonoBehaviour
    {

        [SerializeField] private Button button;
        [SerializeField] private Image fillImage;

        private NosManager nosManager => WarehouseManager.Instance.CurrentCar.NosManager;
        
        private void LateUpdate()
        {
            fillImage.fillAmount = nosManager.AvailableNosPercent;

            button.interactable = nosManager.AvailableNosPercent > NosManager.MinPercentToActivate;
        }
        
    }
}
