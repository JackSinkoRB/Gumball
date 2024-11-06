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
        private readonly bool useServerTime;

        private long currentEpochSeconds => useServerTime ? PlayFabManager.CurrentEpochSecondsSynced : TimeUtils.CurrentEpochSeconds;
        
        private long timeLastStarted
        {
            get => DataManager.Player.Get($"PersistentCooldown.{id}", currentEpochSeconds);
            set => DataManager.Player.Set($"PersistentCooldown.{id}", value);
        }
        public long SecondsSinceStarted => currentEpochSeconds - timeLastStarted;
        public int CyclesCompleted => Mathf.FloorToInt((float)SecondsSinceStarted / CycleDurationSeconds);
        public long SecondsPassedInCurrentCycle => SecondsSinceStarted % CycleDurationSeconds;
        public long SecondsRemainingInCurrentCycle => CycleDurationSeconds - SecondsPassedInCurrentCycle;
        public bool IsPlaying { get; private set; }
        
        public PersistentCooldown(string id, long cycleDurationSeconds, bool useServerTime)
        {
            this.id = id;
            CycleDurationSeconds = cycleDurationSeconds;
            this.useServerTime = useServerTime;
        }

        public void Pause()
        {
            CoroutineHelper.onUnityLateUpdate -= CheckIfCyclesCompleted;
            IsPlaying = false;
        }

        public void Play()
        {
            if (!DataManager.Player.HasKey($"PersistentCooldown.{id}"))
                timeLastStarted = currentEpochSeconds;
            
            CoroutineHelper.onUnityLateUpdate -= CheckIfCyclesCompleted;
            CoroutineHelper.onUnityLateUpdate += CheckIfCyclesCompleted;

            IsPlaying = true;
        }
        
        public void Restart(bool keepRemainingCycle = false)
        {
            timeLastStarted = currentEpochSeconds;
            if (keepRemainingCycle)
                timeLastStarted -= SecondsPassedInCurrentCycle;
                
            Play();
        }

        private void CheckIfCyclesCompleted()
        {
            if (!GameLoaderSceneManager.HasLoaded)
            {
#if UNITY_EDITOR
                if (!IsRunningTests)
#endif
                return;
            }

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
