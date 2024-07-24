using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Near miss in single session")]
    public class NearMissSingleSessionChallengeTracker : ChallengeTracker
    {
        
        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            GameSession.onSessionStart += OnSessionStart;
            
            CoroutineHelper.PerformAfterTrue(
                () => SkillCheckManager.ExistsRuntime, 
                () => SkillCheckManager.Instance.NearMiss.onPerformed += OnNearMiss);
        }

        private void OnSessionStart(GameSession session)
        {
            SetTracker(0); //reset each map
        }

        private void OnNearMiss()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            if (trackers.Count == 0)
                return; //no trackers - no point tracking
            
            Track(1);
        }
        
    }
}
