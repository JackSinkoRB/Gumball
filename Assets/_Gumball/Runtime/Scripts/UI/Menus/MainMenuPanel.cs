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
        
        public void LoadDecalEditor()
        {
            DecalEditor.LoadEditor();
        }

        public void LoadAvatarEditor()
        {
            AvatarEditor.LoadEditor();
        }

        public void LoadWarehouse()
        {
            WarehouseSceneManager.LoadWarehouse();
        }

        public void LoadWorkshop()
        {
            WorkshopSceneManager.LoadWorkshop();
        }
        
        public void OnClickStoreButton()
        {
            PanelManager.GetPanel<StorePanel>().Show();
        }
        
    }
}
