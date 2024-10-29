using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class RevLimitCarAudio : CarAudio
    {
        
        public override void Initialise(CarAudioManager managerBelongsTo)
        {
            base.Initialise(managerBelongsTo);
            
            gameObject.SetActive(false);
        }

        public override void UpdateWhileManagerActive()
        {
            base.UpdateWhileManagerActive();
            
            if (managerBelongsTo == null || managerBelongsTo.CarBelongsTo == null)
                return;

            if (!managerBelongsTo.CarBelongsTo.IsPlayer)
                return; //only do for player

            float leeway = 0.07f * managerBelongsTo.CarBelongsTo.EngineRpmRange.Max; //7%
            if (managerBelongsTo.CarBelongsTo.EngineRpm >= managerBelongsTo.CarBelongsTo.EngineRpmRange.Max - leeway)
            {
                if (!isActive)
                    FadeIn();
            }
            else
            {
                if (isActive)
                    FadeOut();
            }
        }
    }
}
