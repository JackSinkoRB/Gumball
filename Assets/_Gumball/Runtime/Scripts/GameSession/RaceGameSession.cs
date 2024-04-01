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
        
        private int lastFrameCalculatedRacePositions = -1;

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

        public int GetRacePosition(AICar car)
        {
            //only calculate the racer positions once per frame
            if (lastFrameCalculatedRacePositions != Time.frameCount)
            {
                lastFrameCalculatedRacePositions = Time.frameCount;
                
                foreach (AICar racer in CurrentRacers)
                    racer.GetComponent<SplineTravelDistanceCalculator>().CalculateDistanceTraveled();
            
                //sort the cars based on distanceTraveled (descending order)
                racersInPositionOrder = CurrentRacers.OrderByDescending(c => c.GetComponent<SplineTravelDistanceCalculator>().DistanceTraveled).ToArray();
            }
            
            int rank = Array.IndexOf(racersInPositionOrder, car) + 1;
            return rank;
        }
        
    }
}
