using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class MainMenuPanel : AnimatedPanel
    {
        
        public void OnClickChallengesButton()
        {
            PanelManager.GetPanel<ChallengesPanel>().Show();
        }

        public void OnClickWardrobeButton()
        {
            AvatarEditor.LoadEditor();
        }

        public void OnClickGarageButton()
        {
            WarehouseSceneManager.LoadWarehouse();
        }

        public void OnClickStoreButton()
        {
            PanelManager.GetPanel<StorePanel>().Show();
        }
        
    }
}
