using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Challenge Tracker/Jumps")]
    public class JumpsChallengeTracker : ChallengeTracker
    {

        public override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();
            
            CoroutineHelper.PerformAfterTrue(
                () => SkillCheckManager.ExistsRuntime, 
                () => SkillCheckManager.Instance.AirTime.onPerformLanding += OnPerformLanding);
        }

        private void OnPerformLanding()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            if (listeners.Count == 0)
                return; //no listeners - no point tracking
            
            Track(1);
        }
        
    }
}
