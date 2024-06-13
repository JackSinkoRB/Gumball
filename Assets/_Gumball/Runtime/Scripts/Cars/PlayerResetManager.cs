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
            if (WarehouseManager.Instance.CurrentCar == null)
                return;
            
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
            PositionAndRotation unblockedLane = GetUnblockedLane();
            playerCar.Teleport(unblockedLane.Position, unblockedLane.Rotation);
            ChunkMapSceneManager.Instance.DrivingCameraController.SkipTransition();
            
            const float speedAfterResetKmh = 60;
            playerCar.SetSpeed(speedAfterResetKmh);
        }
        
        private PositionAndRotation GetUnblockedLane()
        {
            AICar playerCar = WarehouseManager.Instance.CurrentCar;
            ChunkTrafficManager trafficManager = playerCar.LastKnownChunk.TrafficManager;

            List<TrafficLane> lanes = new List<TrafficLane>(trafficManager.LanesForward);
            
            while (lanes.Count > 0)
            {
                TrafficLane lane = lanes.GetRandom();
                lanes.Remove(lane);

                var (isBlocked, lanePosition) = lane.CanMoveCarToLane(playerCar, ChunkTrafficManager.LaneDirection.FORWARD);
                if (isBlocked)
                    continue;
                
                return lanePosition;
            }
            
            //all are blocked, just use a random one and spawn anyway
            TrafficLane randomLane = trafficManager.LanesForward.GetRandom();
            return randomLane.CanMoveCarToLane(playerCar, ChunkTrafficManager.LaneDirection.FORWARD).Item2;
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
