using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class NosManager : MonoBehaviour
    {

        public const float MinPercentToActivate = 0.1f;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;

        public float AvailableNosPercent { get; private set; }
        public bool IsActivated { get; private set; }

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
        }

        private void Update()
        {
            if (IsActivated)
            {
                if (WarehouseManager.Instance.CurrentCar.IsBraking
                    || WarehouseManager.Instance.CurrentCar.IsHandbrakeEngaged
                    || !WarehouseManager.Instance.CurrentCar.IsAccelerating)
                {
                    Deactivate();
                    return;
                }

                Deplete();
                
                //deactivate if no more NOS
                if (AvailableNosPercent == 0)
                    Deactivate();
            }
            else
            {
                Fill();
            }
        }

        public void SetNos(float percent)
        {
            AvailableNosPercent = Mathf.Clamp01(percent);
        }

        public void RemoveNos(float percent)
        {
            AvailableNosPercent = Mathf.Clamp01(AvailableNosPercent - percent);
        }

        public void AddNos(float percent)
        {
            AvailableNosPercent = Mathf.Clamp01(AvailableNosPercent + percent);
        }

        public void Activate()
        {
            if (IsActivated)
            {
                Debug.LogWarning("Cannot activate NOS as it is already activated.");
                return;
            }
            
            IsActivated = true;

            carBelongsTo.PerformanceSettings.TorqueCurve.SetTemporaryAdditionalTorque(carBelongsTo.NosTorqueAddition);
            
            ChunkMapSceneManager.Instance.DrivingCameraController.CurrentDrivingState.EnableNos(true);
        }
        
        public void Deactivate()
        {
            if (!IsActivated)
            {
                Debug.LogWarning("Cannot deactivate NOS as it is not activated.");
                return;
            }
            
            IsActivated = false;
            
            carBelongsTo.PerformanceSettings.TorqueCurve.SetTemporaryAdditionalTorque(0);
            
            ChunkMapSceneManager.Instance.DrivingCameraController.CurrentDrivingState.EnableNos(false);
        }
        
        private void Deplete()
        {
            float percent = Time.deltaTime / carBelongsTo.NosDepletionRate;
            RemoveNos(percent);
        }
        
        private void Fill()
        {
            float percent = Time.deltaTime / carBelongsTo.NosFillRate;
            AddNos(percent);
        }
        
    }
}
