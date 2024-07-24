using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Driving distance")]
    public class DrivingDistanceChallengeTracker : ChallengeTracker
    {

        private Vector3 previousPosition;

        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            AICar.onPlayerTeleport += OnPlayerTeleport;
        }

        private void OnPlayerTeleport(Vector3 previousPos, Vector3 newPos)
        {
            previousPosition = WarehouseManager.Instance.CurrentCar.transform.position;
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;

            if (!GameSessionManager.ExistsRuntime || GameSessionManager.Instance.CurrentSession == null || !GameSessionManager.Instance.CurrentSession.HasStarted)
                return; //must be in a session

            if (trackers.Count == 0)
                return; //no trackers - no point tracking

            float distanceTravelled = Vector3.Distance(previousPosition, WarehouseManager.Instance.CurrentCar.transform.position);
            Track(Mathf.RoundToInt(distanceTravelled));
            Debug.Log($"Travelled {distanceTravelled}");

            previousPosition = WarehouseManager.Instance.CurrentCar.transform.position;
        }
    }
}
