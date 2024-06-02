using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SplatHolder : MonoBehaviour
    {
        
        [SerializeField] private MinMaxFloat scaleRange = new(1, 1.5f);
        [SerializeField] private float carSpeedForMaxScaleKmh = 100;
        
        private void Update()
        {
            float speedPercent = Mathf.Clamp01(WarehouseManager.Instance.CurrentCar.Speed / carSpeedForMaxScaleKmh);
            transform.localScale = Vector3.one * (scaleRange.Min + (scaleRange.Difference * speedPercent));
        }
        
    }
}
