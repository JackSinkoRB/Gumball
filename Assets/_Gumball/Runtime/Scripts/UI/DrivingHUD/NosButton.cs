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
            
            bool sessionIsActive = GameSessionManager.ExistsRuntime && GameSessionManager.Instance.CurrentSession != null && GameSessionManager.Instance.CurrentSession.InProgress && GameSessionManager.Instance.CurrentSession.HasStarted;
            button.SetInteractable(sessionIsActive && nosManager.AvailableNosPercent > NosManager.MinPercentToActivate);
        }
        
        public void OnPressNosButton()
        {
            if (!InputManager.Instance.CarInput.IsEnabled)
                return;
            
            if (nosManager.IsActivated)
                nosManager.Deactivate();
            else
                nosManager.Activate();
        }
        
    }
}
