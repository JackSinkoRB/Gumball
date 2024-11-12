using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class EngineCarAudio : CarAudio
    {

        /// <summary>
        /// The modifier to apply to cars that aren't the players.
        /// </summary>
        private const float racerVolumeModifier = 0.5f;
        private static readonly MinMaxFloat racerVolumeDistance = new(3, 15);
        private static readonly MinMaxFloat playerVolumeDistance = new(3, 150);
        
        [Header("Engine")]
        [SerializeField] private AnimationCurve volumeRpmModifier;
        [SerializeField] private AnimationCurve pitchRpmModifier;
        [SerializeField] private float smoothSpeed = 5;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private float normalisedRpmInterpolated;

        protected override void InitialiseAsTraffic()
        {
            base.InitialiseAsTraffic();
            
            //traffic has no revving sound
            gameObject.SetActive(false);
        }

        protected override void InitialiseAsRacer()
        {
            base.InitialiseAsRacer();
            
            SetVolumeDistance(racerVolumeDistance);
        }

        protected override void InitialiseAsPlayer()
        {
            base.InitialiseAsPlayer();
            
            SetVolumeDistance(playerVolumeDistance);
        }

        private void LateUpdate()
        {
            if (!source.isPlaying)
                source.Play();
            
            float desiredRpm = Mathf.Clamp01((managerBelongsTo.CarBelongsTo.EngineRpm - managerBelongsTo.CarBelongsTo.EngineRpmRange.Min) / managerBelongsTo.CarBelongsTo.EngineRpmRange.Difference);
            normalisedRpmInterpolated = Mathf.Lerp(normalisedRpmInterpolated, desiredRpm, smoothSpeed * Time.deltaTime);

            float desiredVolume = volumeRpmModifier.Evaluate(normalisedRpmInterpolated);
            if (managerBelongsTo.CarBelongsTo.IsRacer)
                desiredVolume *= racerVolumeModifier;
            
            SetVolumeWithMasterVolume(desiredVolume);
            source.pitch = pitchRpmModifier.Evaluate(normalisedRpmInterpolated);
        }
        
    }
}
