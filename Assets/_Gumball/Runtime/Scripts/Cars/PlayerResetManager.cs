using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class PlayerResetManager : Singleton<PlayerResetManager>
    {

        [Header("Fade")]
        [SerializeField] private CanvasGroup fadeObject;
        [SerializeField] private float fadeDuration;
        [SerializeField] private Ease fadeEaseIn = Ease.InOutSine;
        [SerializeField] private Ease fadeEaseOut = Ease.InOutSine;
        
        [Header("Reset button")]
        [SerializeField] private Button resetButton;
        
        private bool isResetting;
        private Sequence fadeTween;

        protected override void Initialise()
        {
            base.Initialise();
            
            //start faded out:
            fadeObject.DOFade(0, 0);
            fadeObject.gameObject.SetActive(false);
        }
        
        private void LateUpdate()
        {
            CheckIfPlayerIsTooFarFromRoad();
            
            resetButton.gameObject.SetActive(WarehouseManager.Instance.CurrentCar.IsStuck);
        }

        public void ResetToNearestRandomLane()
        {
            if (isResetting)
                return; //already resetting

            GlobalLoggers.AICarLogger.Log($"Resetting player to random lane on nearest spline sample.");
            
            isResetting = true;
            
            fadeObject.gameObject.SetActive(true);
            fadeTween?.Kill();
            fadeTween = DOTween.Sequence()
                .Join(fadeObject.DOFade(1, fadeDuration).SetEase(fadeEaseIn).OnComplete(MoveToNearestRandomLane))
                .Append(fadeObject.DOFade(0, fadeDuration).SetEase(fadeEaseOut))
                .OnComplete(() =>
                {
                    isResetting = false;
                    fadeObject.gameObject.SetActive(false);
                });
        }

        /// <summary>
        /// Gets the nearest spline sample and choses a random lane offset position.
        /// </summary>
        private void MoveToNearestRandomLane()
        {
            AICar playerCar = WarehouseManager.Instance.CurrentCar;

            if (playerCar.LastKnownChunk == null)
            {
                Debug.LogError("Could not move player to nearest random line because the player hasn't been on a chunk yet.");
                return;
            }
            
            //get the desired position
            var (randomLanePosition, randomLaneRotation) = GetUnblockedLane();
            playerCar.Teleport(randomLanePosition, randomLaneRotation);
            ChunkMapSceneManager.Instance.DrivingCameraController.SkipTransition();
            
            const float speedAfterResetKmh = 60;
            playerCar.SetSpeed(speedAfterResetKmh);
        }
        
        private (Vector3, Quaternion) GetUnblockedLane()
        {
            AICar playerCar = WarehouseManager.Instance.CurrentCar;
            ChunkTrafficManager trafficManager = playerCar.LastKnownChunk.TrafficManager;

            List<float> laneDistances = new List<float>(trafficManager.LaneDistancesForward);
            
            var (closestSample, closestDistanceSqr) = playerCar.LastKnownChunk.GetClosestSampleOnSpline(playerCar.transform.position);
            while (laneDistances.Count > 0)
            {
                float laneDistance = laneDistances.GetRandom();
                laneDistances.Remove(laneDistance);
                
                var (lanePosition, laneRotation) = trafficManager.GetLanePosition(closestSample, laneDistance, ChunkTrafficManager.LaneDirection.FORWARD);
                if (!trafficManager.CanSpawnCarAtPosition(lanePosition, laneDistance, true))
                    continue;

                return (lanePosition, laneRotation);
            }
            
            //all are blocked, just use a random one
            return trafficManager.GetLanePosition(closestSample, trafficManager.GetRandomLaneDistance(ChunkTrafficManager.LaneDirection.FORWARD), ChunkTrafficManager.LaneDirection.FORWARD);
        }
        
        /// <summary>
        /// Check if the player is too far from the road spline, and reset them if so.
        /// </summary>
        private void CheckIfPlayerIsTooFarFromRoad()
        {
            bool isLoading = PanelManager.GetPanel<LoadingPanel>().IsShowing;
            if (isLoading)
                return;
            
            if (isResetting)
                return;
            
            Chunk lastKnownChunk = WarehouseManager.Instance.CurrentCar.LastKnownChunk;
            if (lastKnownChunk == null)
                return; //may not be setup yet
            
            var (closestSplineSample, closestDistanceSqr) = lastKnownChunk.GetClosestSampleOnSpline(WarehouseManager.Instance.CurrentCar.transform.position);

            float chunkDistanceLimitSqr = Mathf.Pow(WarehouseManager.Instance.CurrentCar.LastKnownChunk.DistanceFromRoadSplineToResetPlayer, 2);
            if (closestDistanceSqr < chunkDistanceLimitSqr)
                return;
            
            ResetToNearestRandomLane();
        }
        
    }
}
