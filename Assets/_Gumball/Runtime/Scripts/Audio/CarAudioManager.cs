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
        [SerializeField, DisplayInspector] private CarAudio gearChange;
        [SerializeField, DisplayInspector] private CarAudio braking;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;

        private Coroutine checkToEnableCoroutine;
        
        public AICar CarBelongsTo => carBelongsTo;

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;

            foreach (Transform child in transform)
            {
                CarAudio carAudio = child.GetComponent<CarAudio>();
                if (carAudio != null)
                    carAudio.Initialise(this);
            }
        }

        private void LateUpdate()
        {
            CheckToDisable();
        }

        private void OnDestroy()
        {
            if (CoroutineHelper.ExistsRuntime && checkToEnableCoroutine != null)
                CoroutineHelper.Instance.StopCoroutine(checkToEnableCoroutine);
        }

        private void CheckToDisable()
        {
            if (carBelongsTo == null) //not yet initialised
            {
                gameObject.SetActive(false);
                return;
            }

            if (GameSessionManager.Instance.CurrentSession == null || !GameSessionManager.Instance.CurrentSession.HasLoaded)
            {
                gameObject.SetActive(false);
                checkToEnableCoroutine = CoroutineHelper.Instance.PerformAfterTrue(
                    () => GameSessionManager.Instance.CurrentSession != null && GameSessionManager.Instance.CurrentSession.HasLoaded, 
                    () => gameObject.SetActive(true));
            }
        }

    }
}
