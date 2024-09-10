using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class RetrySessionButtonPanel : AnimatedPanel
    {

        public void OnClickRetryButton()
        {
            if (!FuelManager.Instance.HasFuel())
            {
                PanelManager.GetPanel<InsufficientFuelPanel>().Show();
                return;
            }
            
            GameSessionManager.Instance.RestartCurrentSession();
        }

    }
}
