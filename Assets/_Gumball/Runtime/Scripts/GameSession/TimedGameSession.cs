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
        
        public float TimeRemainingSeconds => timeRemainingSeconds;
        
        private TimedSessionPanel sessionPanel => PanelManager.GetPanel<TimedSessionPanel>();
        
        public override string GetName()
        {
            return "Timed";
        }

        protected override IEnumerator LoadSession()
        {
            yield return base.LoadSession();

            InitialiseTimer();
            sessionPanel.Show();
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
            EndSession();
        }

        protected override void OnSessionEnd()
        {
            base.OnSessionEnd();
            
            PanelManager.GetPanel<TimedSessionPanel>().Hide();
            PanelManager.GetPanel<TimedSessionEndPanel>().Show();
            
            WarehouseManager.Instance.CurrentCar.SetAutoDrive(true);
        }
        
        private void InitialiseTimer()
        {
            timerHasStarted = false;
            timeRemainingSeconds = timeAllowedSeconds;
        }

    }
}
