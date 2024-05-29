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
            
            //get the desired position
            var (closestSample, closestSampleDistance) = playerCar.LastKnownChunk.GetClosestSampleOnSpline(playerCar.transform.position);
            var (randomLanePosition, randomLaneRotation) = playerCar.LastKnownChunk.TrafficManager.GetLanePosition(closestSample, playerCar.LastKnownChunk.TrafficManager.GetRandomLaneDistance(ChunkTrafficManager.LaneDirection.FORWARD), ChunkTrafficManager.LaneDirection.FORWARD);

            playerCar.Teleport(randomLanePosition, randomLaneRotation);
            ChunkMapSceneManager.Instance.DrivingCameraController.SkipTransition();
        }
        
    }
}
