using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class AvatarEditorPanel : AnimatedPanel
    {

        public void OnClickBackButton()
        {
            AvatarEditor.Instance.EndSession();
            MainSceneManager.LoadMainScene();
        }

    }
}
