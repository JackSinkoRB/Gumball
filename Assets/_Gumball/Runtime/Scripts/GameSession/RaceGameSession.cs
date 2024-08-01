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

        [SerializeField, ReadOnly] private AICar[] racersInPositionOrderCached;
        
        private int frameLastCachedRacerPositionOrder = -1;

        public AICar[] RacersInPositionOrder
        {
            get
            {
                if (racersInPositionOrderCached == null
                    || frameLastCachedRacerPositionOrder != Time.frameCount)
                {
                    frameLastCachedRacerPositionOrder = Time.frameCount;
                    
                    //sort the cars based on distanceTraveled (descending order)
                    racersInPositionOrderCached = CurrentRacers.Keys.OrderByDescending(
                        c => c.GetComponent<SplineTravelDistanceCalculator>().DistanceTraveled 
                             + c.GetComponent<SplineTravelDistanceCalculator>().InitialDistance).ToArray();
                }

                return racersInPositionOrderCached;
            }
        }
        
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
            int rank = Array.IndexOf(RacersInPositionOrder, car) + 1;
            return rank;
        }
        
    }
}
