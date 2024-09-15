using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class RaceSessionEndPanel : SessionEndPanel
    {
        
        private RaceGameSession raceSession => GameSessionManager.Instance.CurrentSession as RaceGameSession;
        
        protected override void OnShow()
        {
            base.OnShow();

            int finishingRank = raceSession.GetRacePosition(WarehouseManager.Instance.CurrentCar);
            SetPosition(finishingRank);
        }
        
    }
}
