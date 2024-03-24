using System;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] private RacerSessionData[] racerData;

        public override string GetName()
        {
            return "Timed";
        }

        protected override IEnumerator OnSessionLoad()
        {
            yield return InitialiseRacers();
            yield return base.OnSessionLoad();
        }

        private IEnumerator InitialiseRacers()
        {
            List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
            
            foreach (RacerSessionData data in racerData)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(data.AssetReference);
                handle.Completed += h =>
                {
                    RacerCar racer = Instantiate(h.Result, data.StartingPosition.Position, data.StartingPosition.Rotation).GetComponent<RacerCar>();
                    racer.GetComponent<AddressableReleaseOnDestroy>(true).Init(h);
                };
                handles.Add(handle);
            }
            
            yield return new WaitUntil(() => handles.AreAllComplete());
        }
        
    }
}
