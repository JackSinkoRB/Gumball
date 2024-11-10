using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(AudioSource))]
    public class AmbienceCarAudio : Audio
    {

        [SerializeField] private float maxVolume = 1;
        [SerializeField] private MinMaxFloat speedRangeKmh = new(0,30);

        private AICar carBelongsTo;

        private void OnEnable()
        {
            carBelongsTo = transform.GetComponentInAllParents<AICar>();
        }

        private void LateUpdate()
        {
            AdjustVolumeBasedOnSpeed();
        }

        private void AdjustVolumeBasedOnSpeed()
        {
            float speedAsPercent = Mathf.Clamp01((carBelongsTo.SpeedKmh - speedRangeKmh.Min) / speedRangeKmh.Difference);
            source.volume = speedAsPercent * maxVolume;
        }
        
    }
}
