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
        [SerializeField] private TextMeshProUGUI isSlidingLabel;

        private Wheel wheel;
        private CarManager currentCar => PlayerCarManager.Instance.CurrentCar;

        private void OnEnable()
        {
            if (currentCar != null)
                FindWheel();
            
            this.PerformAfterTrue(() => PlayerCarManager.ExistsRuntime, 
                () => PlayerCarManager.Instance.OnCurrentCarChanged += FindWheel);
        }

        private void OnDisable()
        {
            if (PlayerCarManager.ExistsRuntime)
                PlayerCarManager.Instance.OnCurrentCarChanged -= FindWheel;
        }

        private void FindWheel()
        {
            foreach (Wheel currentCarWheel in currentCar.Wheels)
            {
                if (currentCarWheel.isLeft == isLeft && currentCarWheel.isRear == isRear)
                {
                    wheel = currentCarWheel;
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
            isSlidingLabel.gameObject.SetActive(wheel.IsSliding);
        }
    }
}
