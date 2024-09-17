using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class SlipStreamSkillCheck : SkillCheck
    {

        [Header("Slip stream")]
        [Tooltip("The minimum speed the player must be going to enable slip stream.")]
        [SerializeField] private float minSpeedKmh = 50;
        [Tooltip("The width around the cars front position for a racer to be in to trigger slipstreaming.")]
        [SerializeField] private float carWidth = 2; 
        [Tooltip("The max distance in front of the player that can be considered it slip streaming.")]
        [SerializeField] private float maxDistance = 18;
        [Space(5)]
        [SerializeField, ReadOnly] private bool isSlipStreaming;
        [SerializeField, ReadOnly] private float pointsGainedSinceStarted;

        private readonly RaycastHit[] tempHolder = new RaycastHit[1];

        public override void CheckIfPerformed()
        {
            if (WarehouseManager.Instance.CurrentCar.InCollision)
            {
                OnStopSlipStream();
                return;
            }

            if (WarehouseManager.Instance.CurrentCar.SpeedKmh < minSpeedKmh)
            {
                OnStopSlipStream();
                return;
            }

            Vector3 halfExtents = new Vector3(carWidth / 2f, 1, 0);
            Vector3 frontOfCarPosition = WarehouseManager.Instance.CurrentCar.transform.TransformPoint(WarehouseManager.Instance.CurrentCar.FrontOfCarPosition);
            
            int hits = Physics.BoxCastNonAlloc(frontOfCarPosition, halfExtents, WarehouseManager.Instance.CurrentCar.transform.forward, tempHolder, WarehouseManager.Instance.CurrentCar.transform.rotation, maxDistance / 2f, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.RacerCar));

            if (hits > 0)
                OnPerformed();
            else if (isSlipStreaming)
                OnStopSlipStream();

#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(frontOfCarPosition, halfExtents, WarehouseManager.Instance.CurrentCar.transform.rotation, WarehouseManager.Instance.CurrentCar.transform.forward, maxDistance / 2f, hits > 0 ? Color.green : Color.cyan);
#endif
        }
        
        private void OnStartSlipStream()
        {
            if (isSlipStreaming)
                return; //already started
            
            isSlipStreaming = true;
            
            label.gameObject.SetActive(true);

            pointsGainedSinceStarted = 0;
        }
        
        protected override void OnPerformed()
        {
            base.OnPerformed();
            
            if (!isSlipStreaming)
                OnStartSlipStream();
            
            pointsGainedSinceStarted += pointBonus * Time.deltaTime;
            label.text = $"Slipstream +{Mathf.CeilToInt(pointsGainedSinceStarted)}";
        }
        
        private void OnStopSlipStream()
        {
            if (!isSlipStreaming)
                return; //already stopped
            
            isSlipStreaming = false;
            label.gameObject.SetActive(false);
        }

        protected override float GetPointsToAddWhenPerformed()
        {
            return pointBonus * Time.deltaTime;
        }

        protected override float GetNosToAddWhenPerformed()
        {
            return nosBonus * Time.deltaTime;
        }
        
    }
}
