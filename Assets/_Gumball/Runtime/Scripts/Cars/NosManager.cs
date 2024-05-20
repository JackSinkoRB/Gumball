using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class NosManager : MonoBehaviour
    {

        public const float MinPercentToActivate = 0.1f;

        [Tooltip("How long (in seconds) does a full tank of NOS last?")]
        [SerializeField] private float depletionRate = 5; //TODO: might want to make this upgradable (use save data)
        
        [Tooltip("How long (in seconds) does it take to regenerate a full tank of NOS?")]
        [SerializeField] private float fillRate = 30; //TODO: might want to make this upgradable (use save data)

        [Tooltip("The multiplier to apply to the cars torque when NOS is activated.")]
        [SerializeField] private float torqueAddition = 1500; //TODO: might want to make this upgradable (use save data)
        
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
                //deactivate if braking
                if (WarehouseManager.Instance.CurrentCar.IsBraking)
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

            carBelongsTo.SetPeakTorque(carBelongsTo.PeakTorque + torqueAddition);
        }
        
        public void Deactivate()
        {
            if (!IsActivated)
            {
                Debug.LogWarning("Cannot deactivate NOS as it is not activated.");
                return;
            }
            
            IsActivated = false;
            
            carBelongsTo.SetPeakTorque(carBelongsTo.PeakTorque - torqueAddition);
        }
        
        private void Deplete()
        {
            float percent = Time.deltaTime / depletionRate;
            RemoveNos(percent);
        }
        
        private void Fill()
        {
            float percent = Time.deltaTime / fillRate;
            AddNos(percent);
        }
        
    }
}
