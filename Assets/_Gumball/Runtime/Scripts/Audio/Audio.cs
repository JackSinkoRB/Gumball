using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class Audio : MonoBehaviour
    {

        [SerializeField] protected AudioSource source;
        [SerializeField] private bool muteWhenGameIsPaused = true;
        
        [Header("Fade")]
        [SerializeField] private float fadeInDuration = 0.15f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isPaused;
        [SerializeField, ReadOnly] private float defaultVolume;

        private bool isInitialised;
        private bool hasFadedInBefore;
        private Tween currentFadeTween;
        private bool isFadingOut;
        
        protected bool isActive => gameObject.activeInHierarchy && !isFadingOut;

        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        private void Initialise()
        {
            defaultVolume = source.volume;
        }

        private void Update()
        {
            if (muteWhenGameIsPaused)
            {
                if (Time.timeScale == 0)
                    Pause();
                else
                    UnPause();
            }
        }

        public void Pause()
        {
            if (isPaused)
                return;
            
            isPaused = true;
            source.enabled = false;
        }

        public void UnPause()
        {
            if (!isPaused)
                return;
            
            isPaused = false;
            source.enabled = true;
        }
        
        public void SetVolumeDistance(MinMaxFloat volumeDistance)
        {
            source.minDistance = volumeDistance.Min;
            source.maxDistance = volumeDistance.Max;
        }
        
        protected void FadeIn()
        {
            isFadingOut = false;

            gameObject.SetActive(true);

            if (!hasFadedInBefore)
            {
                hasFadedInBefore = true;
                source.volume = 0; //start at 0 volume
            }

            currentFadeTween?.Kill();
            currentFadeTween = source.DOFade(defaultVolume, fadeInDuration);
        }
        
        protected void FadeOut()
        {
            isFadingOut = true;
            
            currentFadeTween?.Kill();
            currentFadeTween = source.DOFade(0, fadeOutDuration);
            currentFadeTween.OnComplete(() =>
            {
                gameObject.SetActive(false);
                isFadingOut = false;
            });
        }
        
    }
}
