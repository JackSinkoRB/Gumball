using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Timed")]
    public class TimedGameSession : GameSession
    {
        
        /// <summary>
        /// How long should the last second actually last? This allows you to extend how long the last second is to make it more 'clutch'.
        /// </summary>
        private const float lastSecondFakedLength = 1.5f;

        [Header("Timed")]
        [SerializeField] private float timeAllowedSeconds = 60;
        [Space(5)]
        [SerializeField, ReadOnly] private float timeRemainingSeconds;
        [SerializeField, ReadOnly] private bool timerHasStarted;

        public float TimeAllowedSeconds => timeAllowedSeconds;
        public float TimeRemainingSeconds => timeRemainingSeconds;

        public override string GetModeDisplayName()
        {
            return "Timed";
        }

        public override Sprite GetModeIcon()
        {
            return GameSessionManager.Instance.TimedIcon;
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<TimedSessionPanel>();
        }
        
        protected override SessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<TimedSessionEndPanel>();
        }
        
        public override ObjectiveUI.FakeChallengeData GetChallengeData()
        {
            return GameSessionManager.Instance.TimeChallengeData;
        }

        protected override IEnumerator LoadSession()
        {
            yield return base.LoadSession();

            InitialiseTimer();
        }

        protected override void OnSessionStart()
        {
            base.OnSessionStart();

            //start the timer
            timerHasStarted = true;
        }
        
        public override void UpdateWhenCurrent()
        {
            base.UpdateWhenCurrent();
            
            if (timerHasStarted)
                DecreaseTimer();
        }

        private void DecreaseTimer()
        {
            //extend the last second
            float timePassedThisFrame = Time.deltaTime;
            if (timeRemainingSeconds < 1)
                timePassedThisFrame /= lastSecondFakedLength;
                    
            timeRemainingSeconds -= timePassedThisFrame;

            if (timeRemainingSeconds < 0)
            {
                timeRemainingSeconds = 0;
                OnTimerExpire();
            }
        }

        private void OnTimerExpire()
        {
            EndSession(ProgressStatus.ATTEMPTED);
        }

        private void InitialiseTimer()
        {
            timerHasStarted = false;
            timeRemainingSeconds = timeAllowedSeconds;
        }

    }
}
