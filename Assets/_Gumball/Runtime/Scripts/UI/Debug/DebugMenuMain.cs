using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class DebugMenuMain : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI speedLabel;
        [SerializeField] private TextMeshProUGUI rpmLabel;
        [SerializeField] private TextMeshProUGUI gearLabel;
        [SerializeField] private TextMeshProUGUI steerInputLabel;
        [SerializeField] private TextMeshProUGUI steerSpeedLabel;
        [SerializeField] private TextMeshProUGUI steeringLabel;
        [SerializeField] private TextMeshProUGUI brakingLabel;
        [SerializeField] private TextMeshProUGUI slipRatioLabel;
        [SerializeField] private TextMeshProUGUI tractionControlLabel;
        [SerializeField] private TextMeshProUGUI stabilityControlLabel;

        private CarManager currentCar => PlayerCarManager.Instance.CurrentCar;
        
        private void LateUpdate()
        {
            if (!PlayerCarManager.ExistsRuntime || currentCar == null)
                return;
            
            speedLabel.text = $"{Mathf.RoundToInt(SpeedUtils.ToKmh(currentCar.Speed)).ToString()} km/h";
            rpmLabel.text = $"{Mathf.RoundToInt(currentCar.drivetrain.rpm)} RPM";
            gearLabel.text = $"Gear {currentCar.drivetrain.Gear}";
            steerInputLabel.text = $"Steer input ({Mathf.RoundToInt(InputManager.SteeringInput * 100f)}%)";
            steerSpeedLabel.text = $"Steer speed ({currentCar.CurrentSteerSpeed:#.#})";
            steeringLabel.text = $"Steering {(currentCar.CurrentSteering.Approximately(0) ? "" : currentCar.CurrentSteering > 0 ? "RIGHT" : "LEFT")} ({Mathf.Abs(Mathf.RoundToInt(currentCar.CurrentSteering * 100f))}%)";
            brakingLabel.text = $"Braking ({Mathf.Abs(Mathf.RoundToInt(currentCar.brake * 100f))}%)";
            slipRatioLabel.text = $"Slip ({Mathf.Abs(Mathf.RoundToInt(currentCar.drivetrain.slipRatio * 100f))}%)";
            tractionControlLabel.gameObject.SetActive(currentCar.drivetrain.TractionControlOn);
            stabilityControlLabel.gameObject.SetActive(currentCar.StabilityControlOn);
        }
    }
}
