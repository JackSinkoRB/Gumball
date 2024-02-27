using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WorkshopPanel : AnimatedPanel
    {
        
        public void OnClickBackButton()
        {
            MainSceneManager.LoadMainScene();
        }
        
    }
}
