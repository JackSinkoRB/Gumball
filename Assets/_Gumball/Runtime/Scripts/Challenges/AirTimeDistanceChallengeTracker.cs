using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Air time distance")]
    public class AirTimeDistanceChallengeTracker : ChallengeTracker
    {

        private Vector3 previousPosition;
        
        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();
            
            AICar.onPlayerTeleport += OnPlayerTeleport;
            
            CoroutineHelper.PerformAfterTrue(
                () => SkillCheckManager.ExistsRuntime, 
                () => SkillCheckManager.Instance.AirTime.onPerformed += OnAirTime);
        }

        private void OnPlayerTeleport(Vector3 previousPos, Vector3 newPos)
        {
            previousPosition = WarehouseManager.Instance.CurrentCar.transform.position;
        }
        
        private void OnAirTime()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            if (trackers.Count == 0)
                return; //no trackers - no point tracking
            
            float distanceTravelled = Vector3.Distance(previousPosition, WarehouseManager.Instance.CurrentCar.transform.position);
            Track(distanceTravelled);
        }

    }
}
