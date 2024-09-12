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

        [SerializeField, ReadOnly] protected List<AICar> racersInPositionOrderCached;
        
        private int frameLastCachedRacerPositionOrder = -1;

        protected List<AICar> RacersInPositionOrder
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
        
        public override string GetModeDisplayName()
        {
            return "Race";
        }
        
        public override Sprite GetModeIcon()
        {
            return GameSessionManager.Instance.RaceIcon;
        }

        public override ObjectiveUI.FakeChallengeData GetChallengeData()
        {
            return GameSessionManager.Instance.RacePositionChallengeData;
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<RaceSessionPanel>();
        }

        protected override SessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<RaceSessionEndPanel>();
        }

        protected override bool IsCompleteOnCrossFinishLine()
        {
            int finishingRank = GetRacePosition(WarehouseManager.Instance.CurrentCar);
            return finishingRank == 1;
        }

        public int GetRacePosition(AICar racer)
        {
            int rank = RacersInPositionOrder.IndexOf(racer) + 1;
            return rank;
        }

        protected virtual void UpdateRacersPositions()
        {
            if (!GameSessionManager.Instance.CurrentSession.InProgress)
                return; //don't keep updating the positions once the session has ended
            
            //sort the cars based on distanceTraveled (descending order)
            racersInPositionOrderCached = CurrentRacers.Keys.OrderByDescending(racer => racer.GetComponent<SplineTravelDistanceCalculator>().DistanceInMap).ToList();
        }
        
    }
}
