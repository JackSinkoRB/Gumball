using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class NosButton : MonoBehaviour
    {

        [SerializeField] private VirtualButton button;
        [SerializeField] private Image fillImage;

        private NosManager nosManager => WarehouseManager.Instance.CurrentCar.NosManager;
        
        private void LateUpdate()
        {
            fillImage.fillAmount = nosManager.AvailableNosPercent;

            button.SetInteractable(nosManager.AvailableNosPercent > NosManager.MinPercentToActivate);
        }
        
        public void OnPressNosButton()
        {
            if (!InputManager.Instance.CarInput.IsEnabled)
                return;
            
            nosManager.Activate();
        }
        
    }
}
