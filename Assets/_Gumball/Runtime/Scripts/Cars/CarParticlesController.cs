using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CarParticlesController : MonoBehaviour
    {

        [SerializeField] private Vector3 positionOffset;
        
        [Header("Speed based")]
        [SerializeField] private ParticleSystem[] speedBasedParticles;
        [SerializeField] private MinMaxFloat speedRangeForParticles = new(100, 300);
        [SerializeField] private float maxTransparency = 0.5f;
        
        //TODO: experiment with changing particle color transparency and amount of particles depending on the player's speed
        
        private void LateUpdate()
        {
            UpdateSpeedBasedValues();
            MoveToPlayer();
        }

        private void UpdateSpeedBasedValues()
        {
            float speedPercent = Mathf.Clamp01((WarehouseManager.Instance.CurrentCar.Speed - speedRangeForParticles.Min) / speedRangeForParticles.Difference);
            
            //do transparency
            float transparency = speedPercent * maxTransparency;
            foreach (ParticleSystem particles in speedBasedParticles)
            {
                ParticleSystem.MainModule main = particles.main;
                main.startColor = main.startColor.color.WithAlphaSetTo(transparency);
            }
        }

        private void MoveToPlayer()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            transform.position = WarehouseManager.Instance.CurrentCar.transform.TransformPoint(positionOffset);
            transform.rotation = Quaternion.LookRotation(-WarehouseManager.Instance.CurrentCar.transform.forward, WarehouseManager.Instance.CurrentCar.transform.up);
        }
        
    }
}
