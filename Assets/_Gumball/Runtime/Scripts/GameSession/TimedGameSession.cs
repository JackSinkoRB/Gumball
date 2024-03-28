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
        [SerializeField] private float raceDistanceMetres = 100;
        [SerializeField] private RacerSessionData[] racerData;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private float timeRemainingSeconds;
        [SerializeField, ReadOnly] private float splineDistanceTraveled;

        /// <summary>
        /// The distance along the spline from the player's starting position to the start of the map. 
        /// </summary>
        private float initialSplineDistance;
        private Coroutine timerCoroutine;
        
        public override string GetName()
        {
            return "Timed";
        }

        protected override IEnumerator OnSessionLoad()
        {
            yield return InitialiseRacers();
            yield return base.OnSessionLoad();

            initialSplineDistance = GetSplineDistanceTraveled();
            SetupFinishLine();

            PanelManager.GetPanel<TimedSessionPanel>().Show();
            InitialiseTimer();
        }

        public override void UpdateWhenCurrent()
        {
            base.UpdateWhenCurrent();

            CheckIfCrossedFinishLine();
        }
        
        private void CheckIfCrossedFinishLine()
        {
            splineDistanceTraveled = GetSplineDistanceTraveled() - initialSplineDistance;
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
            
            //cancel the timer if still running (eg. quit early)
            if (timerCoroutine != null)
                CoroutineHelper.Instance.StopCoroutine(timerCoroutine);
            
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
        
        private void InitialiseTimer()
        {
            timeRemainingSeconds = timeAllowedSeconds;
            
            if (timerCoroutine != null)
                CoroutineHelper.Instance.StopCoroutine(timerCoroutine);
            timerCoroutine = CoroutineHelper.Instance.StartCoroutine(DoTimer());
        }

        private void SetupFinishLine()
        {
            float mapLength = ChunkManager.Instance.CurrentChunkMap.TotalLengthMetres;
            if (mapLength < raceDistanceMetres)
            {
                Debug.LogError($"Could not create finish line as the race distance {raceDistanceMetres} is bigger than the map length {mapLength}.");
                return;
            }
        }
        
        /// <summary>
        /// Gets the distance along the spline from the start of the map to the closest spline sample to the player.
        /// </summary>
        public float GetSplineDistanceTraveled()
        {
            //get the distance in the current chunk
            Chunk currentChunk = ChunkManager.Instance.GetChunkPlayerIsOn();
            if (currentChunk == null || !ChunkManager.Instance.HasLoaded)
                return 0;
            
            int currentChunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(currentChunk);
            Vector3 playerPosition = WarehouseManager.Instance.CurrentCar.transform.position;
            float distanceInCurrentChunk = currentChunk.GetDistanceTravelledAlongSpline(playerPosition);
            
            //get the distance in previous chunks
            float distanceInPreviousChunks = 0;
            for (int index = 0; index < currentChunkIndex; index++)
            {
                ChunkMapData chunkData = ChunkManager.Instance.CurrentChunkMap.GetChunkData(index);
                distanceInPreviousChunks += chunkData.SplineLength;
            }

            return distanceInCurrentChunk + distanceInPreviousChunks;
        }
        
    }
}
