using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Speed camera sprint")]
    public class SpeedCameraSprintSession : GameSession
    {

        [Serializable]
        public class SpeedCamera
        {
            [SerializeField] private float zoneWidth = 25;
            [SerializeField] private float speedLimitKmh = 60;
        }
        
        public override string GetName()
        {
            return "Speed camera sprint";
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<SpeedCameraSprintSessionPanel>();
        }
        
        protected override GameSessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<SpeedCameraSprintSessionEndPanel>();
        }
        
    }
}
