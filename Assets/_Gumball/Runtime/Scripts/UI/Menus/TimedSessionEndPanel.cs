using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class TimedSessionEndPanel : AnimatedPanel
    {

        public void OnClickExitButton()
        {
            MainSceneManager.LoadMainScene();
        }
        
    }
}
