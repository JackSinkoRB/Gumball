using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Race")]
    public class RaceGameSession : GameSession
    {
        
        private RaceSessionPanel sessionPanel => PanelManager.GetPanel<RaceSessionPanel>();
        
        public override string GetName()
        {
            return "Race";
        }
        
        protected override IEnumerator LoadSession()
        {
            yield return base.LoadSession();

            sessionPanel.Show();
        }
        
        protected override void OnSessionEnd()
        {
            base.OnSessionEnd();
            
            PanelManager.GetPanel<RaceSessionEndPanel>().Show();
            
            WarehouseManager.Instance.CurrentCar.SetAutoDrive(true);
        }

    }
}
