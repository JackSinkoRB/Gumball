using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Race")]
    public class RaceGameSession : GameSession
    {

        [SerializeField, ReadOnly] private AICar[] racersInPositionOrder;
        
        public override string GetName()
        {
            return "Race";
        }
        
        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<RaceSessionPanel>();
        }
        
        protected override GameSessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<RaceSessionEndPanel>();
        }

        public int GetRacePosition(AICar car)
        {
            //sort the cars based on distanceTraveled (descending order)
            racersInPositionOrder = CurrentRacers.Keys.OrderByDescending(
                c => c.GetComponent<SplineTravelDistanceCalculator>().DistanceTraveled 
                     + c.GetComponent<SplineTravelDistanceCalculator>().InitialDistance).ToArray();
            
            int rank = Array.IndexOf(racersInPositionOrder, car) + 1;
            return rank;
        }
        
    }
}
