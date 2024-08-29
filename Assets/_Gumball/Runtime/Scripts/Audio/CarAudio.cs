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
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] protected CarAudioManager managerBelongsTo;

        public virtual void Initialise(CarAudioManager managerBelongsTo)
        {
            this.managerBelongsTo = managerBelongsTo;
            
            if (managerBelongsTo.CarBelongsTo.IsTraffic)
                InitialiseAsTraffic();
            else if (managerBelongsTo.CarBelongsTo.IsRacer)
                InitialiseAsRacer();
            else if (managerBelongsTo.CarBelongsTo.IsPlayer)
                InitialiseAsPlayer();
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

    }
}
