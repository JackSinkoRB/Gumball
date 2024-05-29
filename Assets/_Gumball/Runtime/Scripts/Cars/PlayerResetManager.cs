using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Dreamteck.Splines;
using UnityEngine;

namespace Gumball
{
    public class PlayerResetManager : Singleton<PlayerResetManager>
    {

        [SerializeField] private CanvasGroup fadeObject;
        [SerializeField] private float fadeDuration;
        [SerializeField] private Ease fadeEaseIn = Ease.InOutSine;
        [SerializeField] private Ease fadeEaseOut = Ease.InOutSine;
        
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
        }

        public void ResetToNearestRandomLane()
        {
            if (isResetting)
                return; //already resetting

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
            var (closestSample, closestDistanceSqr) = playerCar.LastKnownChunk.GetClosestSampleOnSpline(playerCar.transform.position);
            var (randomLanePosition, randomLaneRotation) = playerCar.LastKnownChunk.TrafficManager.GetLanePosition(closestSample, playerCar.LastKnownChunk.TrafficManager.GetRandomLaneDistance(ChunkTrafficManager.LaneDirection.FORWARD), ChunkTrafficManager.LaneDirection.FORWARD);

            playerCar.Teleport(randomLanePosition, randomLaneRotation);
            ChunkMapSceneManager.Instance.DrivingCameraController.SkipTransition();
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
