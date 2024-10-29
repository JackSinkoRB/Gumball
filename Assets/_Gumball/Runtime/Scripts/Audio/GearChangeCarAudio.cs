using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class GearChangeCarAudio : CarAudio
    {
    
        public override void Initialise(CarAudioManager managerBelongsTo)
        {
            base.Initialise(managerBelongsTo);
            
            gameObject.SetActive(true);
            source.playOnAwake = false;

            if (managerBelongsTo.CarBelongsTo.IsPlayer)
            {
                GameSession.onSessionStart -= OnSessionStart;
                GameSession.onSessionStart += OnSessionStart;

                GameSession.onSessionEnd -= OnSessionEnd;
                GameSession.onSessionEnd += OnSessionEnd;
            }
        }

        private void OnSessionStart(GameSession session)
        {
            managerBelongsTo.CarBelongsTo.onGearChanged += OnGearChanged;
        }
        
        private void OnSessionEnd(GameSession session, GameSession.ProgressStatus progress)
        {
            managerBelongsTo.CarBelongsTo.onGearChanged -= OnGearChanged;
        }
        
        private void OnGearChanged(int previousGear, int currentGear)
        {
            source.Play();
        }
        
    }
}
