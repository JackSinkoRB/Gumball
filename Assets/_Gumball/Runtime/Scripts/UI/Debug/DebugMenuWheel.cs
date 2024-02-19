using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class DebugMenuWheel : MonoBehaviour
    {

        [SerializeField] private bool isRear;
        [SerializeField] private bool isLeft;
        [Space(5)]
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI slipRatioLabel;
        [SerializeField] private TextMeshProUGUI slipAngleLabel;
        [SerializeField] private TextMeshProUGUI slipVeloLabel;
        [SerializeField] private TextMeshProUGUI brakingLabel;
        [SerializeField] private TextMeshProUGUI isSlidingLabel;

        private Wheel wheel;
        private CarManager currentCar => WarehouseManager.Instance.CurrentCar;

        private void OnEnable()
        {
            if (currentCar != null)
                FindWheel(currentCar);
            
            this.PerformAfterTrue(() => PlayerCarManager.ExistsRuntime, 
                () => WarehouseManager.Instance.onCurrentCarChanged += FindWheel);
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= FindWheel;
        }

        private void FindWheel(CarManager newCar)
        {
            foreach (Wheel wheelOnNewCar in newCar.WheelManager.Wheels)
            {
                if (wheelOnNewCar.isLeft == isLeft && wheelOnNewCar.isRear == isRear)
                {
                    wheel = wheelOnNewCar;
                    return;
                }
            }
        }

        private void LateUpdate()
        {
            if (!PlayerCarManager.ExistsRuntime || currentCar == null || wheel == null)
                return;

            nameLabel.text = $"Wheel {(isRear ? "R" : "F")}{(isLeft ? "L" : "R")}";
            slipRatioLabel.text = $"Slip ({Mathf.RoundToInt(wheel.slipRatio * 100f)}%)";
            slipAngleLabel.text = $"Slip angle ({Mathf.RoundToInt(wheel.SlipAngle * 100f)})";
            slipVeloLabel.text = $"Slip velo ({Mathf.RoundToInt(wheel.slipVelo)})";
            brakingLabel.text = $"Braking ({Mathf.RoundToInt(wheel.braking * 100f)}%)";
            isSlidingLabel.gameObject.SetActive(wheel.IsSliding);
        }
    }
}
