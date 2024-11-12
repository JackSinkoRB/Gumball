using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Slip stream distance")]
    public class SlipStreamDistanceChallengeTracker : ChallengeTracker
    {

        private Vector3 previousPosition;

        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            AICar.onPlayerTeleport += OnPlayerTeleport;
            
            CoroutineHelper.PerformAfterTrue(
                () => SkillCheckManager.ExistsRuntime, 
                () => SkillCheckManager.Instance.SlipStream.onPerformed += OnSlipStream);
        }

        public override string GetValueFormatted(float value)
        {
            return SpeedUtils.GetDistanceUserFriendly(value);
        }

        private void OnPlayerTeleport(Vector3 previousPos, Vector3 newPos)
        {
            previousPosition = WarehouseManager.Instance.CurrentCar.transform.position;
        }
        
        private void OnSlipStream()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            if (listeners.Count == 0)
                return; //no listeners - no point tracking
            
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
