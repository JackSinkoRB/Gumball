using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class NosCarAudio : CarAudio
    {

        [SerializeField, ReadOnly] private float defaultVolume;
        
        private Tween currentFadeTween;

        public override void Initialise(CarAudioManager managerBelongsTo)
        {
            base.Initialise(managerBelongsTo);

            defaultVolume = source.volume;
            source.volume = 0;
        }

        protected override void InitialiseAsPlayer()
        {
            base.InitialiseAsPlayer();

            ListenForNos();
        }

        protected override void InitialiseAsRacer()
        {
            base.InitialiseAsRacer();
            
            ListenForNos();
        }

        private void ListenForNos()
        {
            if (managerBelongsTo.CarBelongsTo.NosManager == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(managerBelongsTo.CarBelongsTo.NosManager.IsActivated);
            
            managerBelongsTo.CarBelongsTo.NosManager.onActivate -= OnActivateNos;
            managerBelongsTo.CarBelongsTo.NosManager.onActivate += OnActivateNos;
            
            managerBelongsTo.CarBelongsTo.NosManager.onDeactivate -= OnDeactivateNos;
            managerBelongsTo.CarBelongsTo.NosManager.onDeactivate += OnDeactivateNos;
        }

        private void OnActivateNos()
        {
            FadeIn();
        }

        private void OnDeactivateNos()
        {
            FadeOut();
        }

        private void FadeIn()
        {
            gameObject.SetActive(true);

            const float duration = 0.15f;

            currentFadeTween?.Kill();
            currentFadeTween = source.DOFade(defaultVolume, duration);
        }
        
        private void FadeOut()
        {
            const float duration = 1f;

            source.time = 0; //restart for nos start sound
            
            currentFadeTween?.Kill();
            currentFadeTween = source.DOFade(0, duration);
            currentFadeTween.OnComplete(() => gameObject.SetActive(false));
        }
        
    }
}
