using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class WarehousePanel : AnimatedPanel
    {

        public void OnClickBackButton()
        {
            MainSceneManager.LoadMainScene();
        }
        
    }
}
