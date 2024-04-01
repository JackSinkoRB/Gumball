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
            //sort the cars based on distanceTraveled (descending order)
            racersInPositionOrder = CurrentRacers.OrderByDescending(c => c.GetComponent<SplineTravelDistanceCalculator>().DistanceTraveled).ToArray();
            
            int rank = Array.IndexOf(racersInPositionOrder, car) + 1;
            return rank;
        }
        
    }
}
