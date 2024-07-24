using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Near misses")]
    public class NearMissChallengeTracker : ChallengeTracker
    {

        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();
            
            CoroutineHelper.PerformAfterTrue(
                () => SkillCheckManager.ExistsRuntime, 
                () => SkillCheckManager.Instance.NearMiss.onPerformed += OnNearMiss);
        }

        private void OnNearMiss()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            if (trackers.Count == 0)
                return; //no trackers - no point tracking
            
            Track(1);
            Debug.Log("NEAR MISS");
        }

    }
}
