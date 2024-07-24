using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Challenge Tracker Manager")]
    public class ChallengeTrackerManager : SingletonScriptable<ChallengeTrackerManager>
    {

        [SerializeField] private ChallengeTracker[] trackers;

        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            foreach (ChallengeTracker tracker in trackers)
                tracker.OnInstanceLoaded();
            
            CoroutineHelper.onUnityLateUpdate += Instance.LateUpdate;
        }

        private void LateUpdate()
        {
            foreach (ChallengeTracker tracker in trackers)
                tracker.LateUpdate();
        }
        
    }
}