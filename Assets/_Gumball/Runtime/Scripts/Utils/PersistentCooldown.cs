using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class PersistentCooldown
    {
        
#if UNITY_EDITOR
        public static bool IsRunningTests;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            IsRunningTests = false;
        }
#endif

        public event Action onCycleComplete;
        
        public readonly long CycleDurationSeconds;
        private readonly string id;
        
        private long timeLastStarted
        {
            get => DataManager.Player.Get($"PersistentCooldown.{id}", TimeUtils.CurrentEpochSeconds);
            set => DataManager.Player.Set($"PersistentCooldown.{id}", value);
        }
        public long SecondsSinceStarted => TimeUtils.CurrentEpochSeconds - timeLastStarted;
        public int CyclesCompleted => Mathf.FloorToInt((float)SecondsSinceStarted / CycleDurationSeconds);
        public long SecondsPassedInCurrentCycle => SecondsSinceStarted % CycleDurationSeconds;
        public long SecondsRemainingInCurrentCycle => CycleDurationSeconds - SecondsPassedInCurrentCycle;
        public bool IsPlaying { get; private set; }
        
        public PersistentCooldown(string id, long cycleDurationSeconds)
        {
            this.id = id;
            CycleDurationSeconds = cycleDurationSeconds;
        }

        public void Pause()
        {
            CoroutineHelper.onUnityLateUpdate -= CheckIfCyclesCompleted;
            IsPlaying = false;
        }

        public void Play()
        {
            CoroutineHelper.onUnityLateUpdate -= CheckIfCyclesCompleted;
            CoroutineHelper.onUnityLateUpdate += CheckIfCyclesCompleted;
            
            //ensure coroutinehelper exists
            CoroutineHelper.AssignInstance();
            
            IsPlaying = true;
        }
        
        public void Restart(bool keepRemainingCycle = false)
        {
            timeLastStarted = TimeUtils.CurrentEpochSeconds;
            if (keepRemainingCycle)
                timeLastStarted -= SecondsPassedInCurrentCycle;
                
            Play();
        }

        private void CheckIfCyclesCompleted()
        {
            if (!GameLoaderSceneManager.HasLoaded && !IsRunningTests)
                return;
            
            int completed = CyclesCompleted;
            if (completed == 0)
                return;
            
            //trigger all the completion events
            for (int count = 0; count < completed; count++)
            {
                onCycleComplete?.Invoke();
            }
            
            //remove these cycles
            Restart(true);
        }

    }
}
