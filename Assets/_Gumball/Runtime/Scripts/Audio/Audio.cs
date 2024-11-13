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

        public enum Category
        {
            SFX,
            MUSIC
        }
        
        [SerializeField] protected AudioSource source;
        [SerializeField] protected Category category;
        [SerializeField] private bool muteWhenGameIsPaused = true;
        
        [Header("Fade")]
        [SerializeField] private float fadeInDuration = 0.15f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isPaused;
        [SerializeField, ReadOnly] private float defaultVolume;
        [SerializeField, ReadOnly] private float currentVolumeWithoutMaster;

        private bool isInitialised;
        private bool hasFadedInBefore;
        private Tween currentFadeTween;
        private bool isFadingOut;
        
        protected bool isActive => gameObject.activeInHierarchy && !isFadingOut;

        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
            
            ListenForVolumeSettingChange();
        }

        private void OnDisable()
        {
            StopListeningForVolumeSettingChange();
        }

        private void Initialise()
        {
            defaultVolume = source.volume;
            SetVolumeWithMasterVolume(defaultVolume);
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

        public void SetVolumeWithMasterVolume(float volumePercent)
        {
            currentVolumeWithoutMaster = volumePercent;
            source.volume = volumePercent * GetMasterVolumePercent();
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
                SetVolumeWithMasterVolume(0); //start at 0 volume
            }

            currentFadeTween?.Kill();
            currentFadeTween = source.DOFade(defaultVolume * GetMasterVolumePercent(), fadeInDuration);
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
        
        protected float GetMasterVolumePercent()
        {
            return category switch
            {
                Category.SFX => SFXVolumeSetting.SFXVolumePercent,
                Category.MUSIC => MusicVolumeSetting.MusicVolumePercent,
                _ => throw new NotImplementedException()
            };
        }
        
        protected void ListenForVolumeSettingChange()
        {
            switch (category)
            {
                case Category.SFX:
                    SFXVolumeSetting.onVolumeSettingChange += OnVolumeSettingChange;
                    break;
                case Category.MUSIC:
                    MusicVolumeSetting.onVolumeSettingChange += OnVolumeSettingChange;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        
        protected void StopListeningForVolumeSettingChange()
        {
            switch (category)
            {
                case Category.SFX:
                    SFXVolumeSetting.onVolumeSettingChange -= OnVolumeSettingChange;
                    break;
                case Category.MUSIC:
                    MusicVolumeSetting.onVolumeSettingChange -= OnVolumeSettingChange;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void OnVolumeSettingChange(float newVolume)
        {
            SetVolumeWithMasterVolume(currentVolumeWithoutMaster);
        }
        
    }
}
