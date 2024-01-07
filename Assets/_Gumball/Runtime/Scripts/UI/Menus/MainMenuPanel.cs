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
        
        /// <remarks>Temporary method to load the testing map. Should be replaced by the map picker UI to load a specific map.</remarks>
        public void LoadTestMapDrivingScene()
        {
            MapDrivingSceneManager.LoadMapDrivingScene(ChunkManager.Instance.TestingMap);
        }

        public void LoadDecalEditor()
        {
            DecalEditor.LoadDecalEditor();
        }
        
    }
}
