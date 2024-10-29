using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class BrakingCarAudio : CarAudio
    {

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
        }

        private void OnSessionEnd(GameSession session, GameSession.ProgressStatus progress)
        {
            managerBelongsTo.CarBelongsTo.onStartBraking -= OnStartBraking;
            managerBelongsTo.CarBelongsTo.onStopBraking -= OnStopBraking;
        }
        
        private void OnStartBraking()
        {
            if (managerBelongsTo.CarBelongsTo.IsStationary)
                return;
            
            FadeIn();
        }
        
        private void OnStopBraking()
        {
            FadeOut();
        }
        
    }
}
