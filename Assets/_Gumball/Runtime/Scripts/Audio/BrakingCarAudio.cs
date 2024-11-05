using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class BrakingCarAudio : CarAudio
    {

        private Coroutine stopBrakingCoroutine;
        
        public override void Initialise(CarAudioManager managerBelongsTo)
        {
            base.Initialise(managerBelongsTo);
            
            gameObject.SetActive(false);
            
            if (managerBelongsTo.CarBelongsTo.IsPlayer)
            {
                GameSession.onSessionStart += OnSessionStart;
                GameSession.onSessionEnd += OnSessionEnd;
            }
        }

        private void OnDestroy()
        {
            GameSession.onSessionStart -= OnSessionStart;
            GameSession.onSessionEnd -= OnSessionEnd;

            if (managerBelongsTo != null && managerBelongsTo.CarBelongsTo != null)
            {
                managerBelongsTo.CarBelongsTo.onStartBraking -= OnStartBraking;
                managerBelongsTo.CarBelongsTo.onStopBraking -= OnStopBraking;
            }
        }

        private void OnSessionStart(GameSession session)
        {
            managerBelongsTo.CarBelongsTo.onStartBraking += OnStartBraking;
            managerBelongsTo.CarBelongsTo.onStopBraking += OnStopBraking;
            managerBelongsTo.CarBelongsTo.onEngageHandbrake += OnStartBraking;
            managerBelongsTo.CarBelongsTo.onDisengageHandbrake += OnStopBraking;
        }

        private void OnSessionEnd(GameSession session, GameSession.ProgressStatus progress)
        {
            managerBelongsTo.CarBelongsTo.onStartBraking -= OnStartBraking;
            managerBelongsTo.CarBelongsTo.onStopBraking -= OnStopBraking;
            managerBelongsTo.CarBelongsTo.onEngageHandbrake -= OnStartBraking;
            managerBelongsTo.CarBelongsTo.onDisengageHandbrake -= OnStopBraking;
        }

        public override void UpdateWhileManagerActive()
        {
            base.UpdateWhileManagerActive();

            const float minSpeedKmh = 10;
            if (managerBelongsTo.CarBelongsTo.SpeedKmh < minSpeedKmh)
                FadeOut();
        }

        private void OnStartBraking()
        {
            if (stopBrakingCoroutine != null)
                StopCoroutine(stopBrakingCoroutine);
            
            if (!managerBelongsTo.CarBelongsTo.IsStationary)
                FadeIn();
        }
        
        private void OnStopBraking()
        {
            if (stopBrakingCoroutine != null)
                StopCoroutine(stopBrakingCoroutine);
            stopBrakingCoroutine = this.PerformAfterTrue(() => !managerBelongsTo.CarBelongsTo.IsHandbrakeEasingOff, FadeOut);
        }
        
    }
}
