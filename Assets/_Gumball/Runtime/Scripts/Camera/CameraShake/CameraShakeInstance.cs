using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    [Serializable]
    public class CameraShakeInstance
    {
        
        public enum State
        {
            Inactive,
            FadingIn,
            FadingOut,
            Sustaining
        }
        
        [Tooltip("The intensity of the shake.")]
        [SerializeField] private float magnitude;
        [Tooltip("Roughness of the shake.")]
        [SerializeField] private float roughness;
        [Tooltip("How much influence this shake has over the local position axes of the camera.")]
        [SerializeField] private Vector3 positionInfluence;
        [Tooltip("How much influence this shake has over the local rotation axes of the camera.")]
        [SerializeField] private Vector3 rotationInfluence;
        [SerializeField] private float fadeInDuration;
        [SerializeField] private float fadeOutDuration;
        [Tooltip("Does it start fading out as soon as it has faded in, or does StartFadeOut() need to be called?")]
        [SerializeField] private bool fadeOutInstantly;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private State currentState = State.Inactive;
        [SerializeField, ReadOnly] private float magnitudeModifier = 1;
        
        private bool isInitialised;
        private float fadeAmount;
        private Tween currentFadeTween;
        private Vector3 tick;
        
        public State CurrentState => currentState;
        public float CurrentMagnitude => magnitude * magnitudeModifier * fadeAmount;
        public Vector3 PositionInfluence => positionInfluence;
        public Vector3 RotationInfluence => rotationInfluence;

        public CameraShakeInstance(float magnitude, float roughness, Vector3 positionInfluence, Vector3 rotationInfluence, float fadeInDuration, float fadeOutDuration)
        {
            this.magnitude = magnitude;
            this.roughness = roughness;
            this.positionInfluence = positionInfluence;
            this.rotationInfluence = rotationInfluence;
            this.fadeInDuration = fadeInDuration;
            this.fadeOutDuration = fadeOutDuration;
        }

        private void Initialise()
        {
            isInitialised = true;

            tick = new Vector3(Random.Range(-100, 100),Random.Range(-100, 100),Random.Range(-100, 100));
        }

        public Vector3 UpdateShake()
        {
            if (!isInitialised)
                Initialise();
            
            Vector3 amount = new(Mathf.PerlinNoise(tick.x, 0) - 0.5f, 
                amount.y = Mathf.PerlinNoise(tick.y, 0) - 0.5f, 
                amount.z = Mathf.PerlinNoise(tick.z, 0) - 0.5f);

            tick += Vector3.one * (Time.deltaTime * roughness * fadeAmount);
            
            return amount * CurrentMagnitude;
        }

        public void StartFadeIn()
        {
            magnitudeModifier = 1; //reset
            
            if (!isInitialised)
                Initialise();

            currentFadeTween?.Kill();
            
            if (fadeInDuration == 0)
            {
                fadeAmount = 1;
                currentState = State.Sustaining;
                if (fadeOutInstantly)
                    StartFadeOut();
            }
            else
            {
                currentState = State.FadingIn;
                currentFadeTween = DOTween.To(() => fadeAmount, x => fadeAmount = x, 1, fadeInDuration).OnComplete(() =>
                {
                    currentState = State.Sustaining;
                    if (fadeOutInstantly)
                        StartFadeOut();
                });
            }
        }
        
        public void StartFadeOut()
        {
            if (!isInitialised)
                Initialise();
            
            currentFadeTween?.Kill();
            
            if (fadeOutDuration == 0)
            {
                fadeAmount = 0;
                currentState = State.Inactive;
            }
            else
            {
                currentState = State.FadingOut;
                currentFadeTween = DOTween.To(() => fadeAmount, x => fadeAmount = x, 0, fadeOutDuration)
                    .OnComplete(() => currentState = State.Inactive);
            }
        }

        public void DoShake(float magnitudeModifier = 1)
        {
            StartFadeIn();
            CameraShaker.Instance.TrackShake(this);
            
            this.magnitudeModifier = magnitudeModifier;
        }
        
    }
}
