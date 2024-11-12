using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class NosCarAudio : CarAudio
    {
        
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
            source.time = 0; //restart for nos start sound
            FadeOut();
        }

    }
}
