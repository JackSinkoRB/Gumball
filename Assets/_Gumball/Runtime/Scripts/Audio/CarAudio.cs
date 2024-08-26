using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CarAudio : MonoBehaviour
    {

        [SerializeField] private AudioSource source;
        [SerializeField] private AnimationCurve volumeRpmModifier;
        [SerializeField] private AnimationCurve pitchRpmModifier;
        [SerializeField] private float smoothSpeed = 5;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;
        [SerializeField, ReadOnly] private float normalisedRpmInterpolated;
        
        private bool isInitialised;
        
        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        private void Initialise()
        {
            isInitialised = true;
            
            carBelongsTo = transform.GetComponentInAllParents<AICar>();
        }

        private void LateUpdate()
        {
            if (!source.isPlaying)
                source.Play();
            
            float desiredRpm = Mathf.Clamp01((carBelongsTo.EngineRpm - carBelongsTo.EngineRpmRange.Min) / carBelongsTo.EngineRpmRange.Difference);
            normalisedRpmInterpolated = Mathf.Lerp(normalisedRpmInterpolated, desiredRpm, smoothSpeed * Time.deltaTime);
            
            source.volume = volumeRpmModifier.Evaluate(normalisedRpmInterpolated);
            source.pitch = pitchRpmModifier.Evaluate(normalisedRpmInterpolated);
        }
        
    }
}
