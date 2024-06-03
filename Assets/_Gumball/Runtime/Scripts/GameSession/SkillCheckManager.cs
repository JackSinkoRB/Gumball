using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using UnityEngine;

namespace Gumball
{
    public class SkillCheckManager : MonoBehaviour
    {
        
        [Header("Near miss")]
        [Tooltip("The max distance around a traffic car to consider a near miss.")]
        [SerializeField] private float nearMissMaxDistance = 1;
        [SerializeField] private float nearMissNosBonus;

        [Header("Slip stream")]
        [SerializeField] private TextMeshProUGUI slipStreamLabel;
        [Tooltip("The minimum speed the player must be going to enable slip stream.")]
        [SerializeField] private float minSpeedForSlipStreamKmh = 25;
        [Tooltip("The width around the cars front position for a racer to be in to trigger slipstreaming.")]
        [SerializeField] private float slipStreamWidth = 1; 
        [Tooltip("The max distance in front of the player that can be considered it slip streaming.")]
        [SerializeField] private float slipStreamMaxDistance = 3;
        [Tooltip("A nos bonus (measured in percent) to give each second while the player is within another racer cars slip stream bounds.")]
        [SerializeField, Range(0,1)] private float slipStreamNosBonus = 0.1f;
        [SerializeField] private float slipStreamPointBonus = 1;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isSlipStreaming;
        [SerializeField, ReadOnly] private float pointsGainedSinceSlipStreamStarted;

        [Header("Air time")]
        [SerializeField] private float minimumSpeedForAirTimeKmh = 25;
        [Tooltip("A nos bonus (measured in percent) to give each second while the player has 0 wheels touching the ground and moving at the minimum speed.")]
        [SerializeField] private float airTimeNosBonus = 1;

        [Header("Landing")]
        [SerializeField] private float minTimeInAirForLandingPoints = 0.3f;
        [SerializeField] private float landingNosBonus = 10;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private float currentPoints;

        private readonly RaycastHit[] tempArrayHolder = new RaycastHit[1];
        
        public float CurrentPoints => currentPoints;

        private void OnEnable()
        {
            //start disabled
            slipStreamLabel.gameObject.SetActive(false);
        }

        private void Update()
        {
            CheckForNearMiss();
            CheckForSlipStream();
            CheckForAirTime();
        }

        private void CheckForNearMiss()
        {
            //TODO: do overlay box each frame and check for traffic cars
            // - if player was in radius last frame but is no longer in the radius, and there was not a collision - it is a near miss
        }
        
        private void CheckForSlipStream()
        {
            if (WarehouseManager.Instance.CurrentCar.InCollision)
            {
                OnStopSlipStream();
                return;
            }

            if (WarehouseManager.Instance.CurrentCar.Speed < minSpeedForSlipStreamKmh)
            {
                OnStopSlipStream();
                return;
            }

            Vector3 bounds = new Vector3(slipStreamWidth / 2f, 1, 0);
            Vector3 frontOfCarPosition = WarehouseManager.Instance.CurrentCar.transform.TransformPoint(WarehouseManager.Instance.CurrentCar.FrontOfCarPosition);
            
            int hits = Physics.BoxCastNonAlloc(frontOfCarPosition, bounds, WarehouseManager.Instance.CurrentCar.transform.forward, tempArrayHolder, WarehouseManager.Instance.CurrentCar.transform.rotation, slipStreamMaxDistance / 2f, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.RacerCar));

            if (hits > 0)
                OnPerformSlipStream();
            else if (isSlipStreaming)
                OnStopSlipStream();

#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(frontOfCarPosition, bounds, WarehouseManager.Instance.CurrentCar.transform.rotation, WarehouseManager.Instance.CurrentCar.transform.forward, slipStreamMaxDistance / 2f, hits > 0 ? Color.green : Color.cyan);
#endif
        }

        private void CheckForAirTime()
        {
            //TODO: if all wheels are off the ground
            //set inAir = true
            //timeInAir++
            
            // - if goes from inAir to !inAir and timeInAir > minTimeInAirForLandingPoints - do landing
        }

        private void OnPerformNearMiss()
        {
            
        }

        private void OnStartSlipStream()
        {
            if (isSlipStreaming)
                return; //already started
            
            isSlipStreaming = true;
            
            slipStreamLabel.gameObject.SetActive(true);

            pointsGainedSinceSlipStreamStarted = 0;
        }
        
        private void OnPerformSlipStream()
        {
            if (!isSlipStreaming)
                OnStartSlipStream();
            
            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(slipStreamNosBonus * Time.deltaTime);
            
            pointsGainedSinceSlipStreamStarted += slipStreamPointBonus * Time.deltaTime;
            slipStreamLabel.text = $"Slipstream +{Mathf.CeilToInt(pointsGainedSinceSlipStreamStarted)}";
        }

        private void OnStopSlipStream()
        {
            if (!isSlipStreaming)
                return; //already stopped
            
            isSlipStreaming = false;
            slipStreamLabel.gameObject.SetActive(false);
        }

        private void OnPerformAirTime()
        {
            
        }

        private void OnPerformLanding()
        {
            
        }

    }
}
