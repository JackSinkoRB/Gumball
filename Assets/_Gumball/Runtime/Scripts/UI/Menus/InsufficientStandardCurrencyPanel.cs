using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class InsufficientStandardCurrencyPanel : AnimatedPanel
    {

        public void OnClickStoreButton()
        {
            Hide();
            PanelManager.GetPanel<StorePanel>().Show();
        }
        
    }
}