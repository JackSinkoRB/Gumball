using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CarAudio : MonoBehaviour
    {

        [SerializeField] protected AudioSource source;

        [Header("Fade")]
        [SerializeField] private float fadeInDuration = 0.15f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] protected CarAudioManager managerBelongsTo;
        [SerializeField, ReadOnly] private float defaultVolume;

        private bool hasFadedInBefore;
        private Tween currentFadeTween;
        private bool isFadingOut;

        protected bool isActive => gameObject.activeInHierarchy && !isFadingOut;
        
        public virtual void Initialise(CarAudioManager managerBelongsTo)
        {
            this.managerBelongsTo = managerBelongsTo;
            
            if (managerBelongsTo.CarBelongsTo.IsTraffic)
                InitialiseAsTraffic();
            else if (managerBelongsTo.CarBelongsTo.IsRacer)
                InitialiseAsRacer();
            else if (managerBelongsTo.CarBelongsTo.IsPlayer)
                InitialiseAsPlayer();
                
            defaultVolume = source.volume;
        }

        public virtual void UpdateWhileManagerActive()
        {
            
        }

        public void SetVolumeDistance(MinMaxFloat volumeDistance)
        {
            source.minDistance = volumeDistance.Min;
            source.maxDistance = volumeDistance.Max;
        }
        
        protected virtual void InitialiseAsTraffic()
        {
            
        }

        protected virtual void InitialiseAsRacer()
        {
            
        }

        protected virtual void InitialiseAsPlayer()
        {
            
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
