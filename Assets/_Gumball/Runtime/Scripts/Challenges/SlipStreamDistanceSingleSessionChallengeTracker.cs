using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Slip stream distance in single session")]
    public class SlipStreamDistanceSingleSessionChallengeTracker : ChallengeTracker
    {

        private Vector3 previousPosition;

        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            AICar.onPlayerTeleport += OnPlayerTeleport;
            GameSession.onSessionStart += OnSessionStart;
            
            CoroutineHelper.PerformAfterTrue(
                () => SkillCheckManager.ExistsRuntime, 
                () => SkillCheckManager.Instance.SlipStream.onPerformed += OnSlipStream);
        }

        private void OnSessionStart(GameSession session)
        {
            SetTracker(0); //reset each map
        }

        private void OnPlayerTeleport(Vector3 previousPos, Vector3 newPos)
        {
            previousPosition = WarehouseManager.Instance.CurrentCar.transform.position;
        }
        
        private void OnSlipStream()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            if (trackers.Count == 0)
                return; //no trackers - no point tracking
            
            float distanceTravelled = Vector3.Distance(previousPosition, WarehouseManager.Instance.CurrentCar.transform.position);
            Track(distanceTravelled);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            previousPosition = WarehouseManager.Instance.CurrentCar.transform.position;
        }
        
    }
}
