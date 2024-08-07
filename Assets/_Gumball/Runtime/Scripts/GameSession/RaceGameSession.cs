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

        [SerializeField, ReadOnly] protected AICar[] racersInPositionOrderCached;
        
        private int frameLastCachedRacerPositionOrder = -1;

        protected AICar[] RacersInPositionOrder
        {
            get
            {
                if (racersInPositionOrderCached == null
                    || frameLastCachedRacerPositionOrder != Time.frameCount)
                {
                    frameLastCachedRacerPositionOrder = Time.frameCount;
                    UpdateRacersPositions();
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
        
        public int GetRacePosition(AICar racer)
        {
            int rank = Array.IndexOf(RacersInPositionOrder, racer) + 1;
            return rank;
        }

        protected virtual void UpdateRacersPositions()
        {
            //sort the cars based on distanceTraveled (descending order)
            racersInPositionOrderCached = CurrentRacers.Keys.OrderByDescending(racer => racer.GetComponent<SplineTravelDistanceCalculator>().DistanceInMap).ToArray();
        }
        
    }
}
