using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Timed")]
    public class TimedGameSession : GameSession
    {

        [Serializable]
        public struct RacerSessionData
        {
            [SerializeField] private AssetReferenceGameObject assetReference;
            [SerializeField] private PositionAndRotation startingPosition;

            public AssetReferenceGameObject AssetReference => assetReference;
            public PositionAndRotation StartingPosition => startingPosition;
        }

        [SerializeField] private float timeAllowedSeconds = 60;
        [SerializeField] private RacerSessionData[] racerData;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private float timeRemainingSeconds;
        
        private Coroutine timerCoroutine;
        
        public override string GetName()
        {
            return "Timed";
        }

        protected override IEnumerator OnSessionLoad()
        {
            yield return InitialiseRacers();
            yield return base.OnSessionLoad();

            PanelManager.GetPanel<TimedSessionPanel>().Show();
            InitialiseTimer();
        }

        private void InitialiseTimer()
        {
            timeRemainingSeconds = timeAllowedSeconds;
            
            if (timerCoroutine != null)
                CoroutineHelper.Instance.StopCoroutine(timerCoroutine);
            timerCoroutine = CoroutineHelper.Instance.StartCoroutine(DoTimer());
        }
        
        private IEnumerator DoTimer()
        {
            while (timeRemainingSeconds > 0)
            {
                const float timeRemainingForMs = 10; //if timer goes below this value, show the milliseconds
                PanelManager.GetPanel<TimedSessionPanel>().TimerLabel.text = TimeSpan.FromSeconds(timeRemainingSeconds).ToPrettyString(timeRemainingSeconds < timeRemainingForMs, precise: false);

                yield return null;
                
                timeRemainingSeconds -= Time.deltaTime;

                if (timeRemainingSeconds < 0)
                    timeRemainingSeconds = 0;
            }
            
            EndSession();
        }

        public override void EndSession()
        {
            base.EndSession();
            
            //TODO: reward/race end screen
            MainSceneManager.LoadMainScene();
        }

        private IEnumerator InitialiseRacers()
        {
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
            
            foreach (RacerSessionData data in racerData)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(data.AssetReference);
                handle.Completed += h =>
                {
                    AICar racer = Instantiate(h.Result, data.StartingPosition.Position, data.StartingPosition.Rotation).GetComponent<AICar>();
                    racer.GetComponent<AddressableReleaseOnDestroy>(true).Init(h);
                    
                    racer.SetAutoDrive(true);
                };
                handles.Add(handle);
            }
            
            yield return new WaitUntil(() => handles.AreAllComplete());
        }
        
    }
}
