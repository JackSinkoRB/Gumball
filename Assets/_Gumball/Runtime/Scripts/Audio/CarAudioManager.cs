using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CarAudioManager : MonoBehaviour
    {
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;
        [SerializeField, ReadOnly] private CarAudio[] audioInstances;
        
        private Coroutine checkToEnableCoroutine;
        
        public AICar CarBelongsTo => carBelongsTo;

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;

            audioInstances = transform.GetComponentsInAllChildren<CarAudio>().ToArray();
            
            foreach (CarAudio carAudio in audioInstances)
                carAudio.Initialise(this);
        }

        private void LateUpdate()
        {
            foreach (CarAudio carAudio in audioInstances)
                carAudio.UpdateWhileManagerActive();
            
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
