using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class GameSessionMapPanel : AnimatedPanel
    {

        public void OnClickBackButton()
        {
            GameSessionManager.Instance.CurrentSession.EndSession();
            MainSceneManager.LoadMainScene();
        }
        
    }
}
