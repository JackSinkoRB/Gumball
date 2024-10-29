using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class WindCarAudio : CarAudio
    {

        [Header("Wind")]
        [SerializeField] private float maxVolume = 1;
        [SerializeField] private MinMaxFloat speedRangeKmh = new(100,200);
        
        public override void Initialise(CarAudioManager managerBelongsTo)
        {
            base.Initialise(managerBelongsTo);
            
            gameObject.SetActive(managerBelongsTo.CarBelongsTo.IsPlayer);
        }
        
        public override void UpdateWhileManagerActive()
        {
            base.UpdateWhileManagerActive();

            float speedAsPercent = Mathf.Clamp01((managerBelongsTo.CarBelongsTo.SpeedKmh - speedRangeKmh.Min) / speedRangeKmh.Difference);
            source.volume = speedAsPercent * maxVolume;
        }

    }
}
