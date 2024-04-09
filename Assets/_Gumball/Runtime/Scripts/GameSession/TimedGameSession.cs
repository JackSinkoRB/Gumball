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

        [Header("Timed")]
        [SerializeField] private float timeAllowedSeconds = 60;
        [Space(5)]
        [SerializeField, ReadOnly] private float timeRemainingSeconds;

        public float TimeRemainingSeconds => timeRemainingSeconds;
        
        private TimedSessionPanel sessionPanel => PanelManager.GetPanel<TimedSessionPanel>();
        
        public override string GetName()
        {
            return "Timed";
        }

        protected override IEnumerator LoadSession()
        {
            yield return base.LoadSession();

            sessionPanel.Show();
            InitialiseTimer();
        }

        public override void UpdateWhenCurrent()
        {
            base.UpdateWhenCurrent();
            
            DecreaseTimer();
        }

        private void DecreaseTimer()
        {
            timeRemainingSeconds -= Time.deltaTime;

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
            timeRemainingSeconds = timeAllowedSeconds;
        }

    }
}
