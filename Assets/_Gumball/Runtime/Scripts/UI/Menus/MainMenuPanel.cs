using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class MainMenuPanel : AnimatedPanel
    {

        public void OnClickSettingsButton()
        {
            PanelManager.GetPanel<SettingsPanel>().Show();
        }

        public void ShowMapPanel()
        {
            MapSceneManager.LoadMapScene();
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
        
    }
}
