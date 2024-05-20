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
        
        /// <summary>
        /// How long (in seconds) does a full tank of NOS last?
        /// </summary>
        private const float depletionRate = 3; //TODO: might want to make this upgradable (use save data)
        /// <summary>
        /// How long (in seconds) does it take to regenerate a full tank of NOS?
        /// </summary>
        private const float fillRate = 30; //TODO: might want to make this upgradable (use save data)

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
                Deplete();
                if (AvailableNosPercent == 0)
                    IsActivated = false;
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
            IsActivated = true;
        }

        public void Deactivate()
        {
            IsActivated = false;
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
