using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CarAudioManager : MonoBehaviour
    {

        [Header("Engine")]
        [SerializeField, DisplayInspector] private CarAudio revving;
        [SerializeField, DisplayInspector] private CarAudio nos;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;

        public AICar CarBelongsTo => carBelongsTo;

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
            
            revving.Initialise(this);
            nos.Initialise(this);
        }

        private void LateUpdate()
        {
            if (carBelongsTo == null)
                gameObject.SetActive(false);
        }

    }
}
