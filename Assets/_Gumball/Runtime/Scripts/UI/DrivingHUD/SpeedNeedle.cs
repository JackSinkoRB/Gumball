using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SpeedNeedle : MonoBehaviour
    {

        [SerializeField] private MinMaxFloat minMaxRotation;
        [SerializeField] private float maxSpeedDisplayedKmh = 300;
        [SerializeField] private float maxSpeedDisplayedMph = 200;
        [SerializeField] private float needleLerpSpeed = 10;

        private float maxSpeedDisplayed => UnitOfSpeedSetting.UseMiles ? maxSpeedDisplayedMph : maxSpeedDisplayedKmh;
        
        private void LateUpdate()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
            {
                UpdateNeedleAsPercent(0);
                return;
            }

            float currentSpeed = UnitOfSpeedSetting.UseMiles ? SpeedUtils.FromKmToMiles(WarehouseManager.Instance.CurrentCar.Speed) : WarehouseManager.Instance.CurrentCar.Speed;
            float rpmAsPercent = Mathf.Clamp01(currentSpeed / maxSpeedDisplayed);
            UpdateNeedleAsPercent(rpmAsPercent);
        }
        
        private void UpdateNeedleAsPercent(float percent)
        {
            float desiredRotation = minMaxRotation.Max - (percent * minMaxRotation.Difference);
            float rotationLerped = Mathf.LerpAngle(transform.eulerAngles.z, desiredRotation, Time.deltaTime * needleLerpSpeed);
            
            transform.eulerAngles = transform.eulerAngles.SetZ(rotationLerped);
        }
        
    }
}
