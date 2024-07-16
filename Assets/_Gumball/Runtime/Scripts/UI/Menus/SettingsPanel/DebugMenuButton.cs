using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DebugMenuButton : MonoBehaviour
    {

        public void OnClickButton()
        {
            PanelManager.GetPanel<DebugPanel>().Show();
        }
        
    }
}
