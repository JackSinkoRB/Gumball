using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class RevsUI : MonoBehaviour
    {

        [SerializeField] private RectTransform needle;
        [SerializeField] private float maxRpmDisplayed = 9000;
        [SerializeField] private MinMaxFloat minMaxNeedleRotation;
        [SerializeField] private float needleLerpSpeed = 10;
        
        private bool carExists => PlayerCarManager.ExistsRuntime && PlayerCarManager.Instance.CurrentCar != null;

        private void LateUpdate()
        {
            if (!carExists)
            {
                UpdateNeedleAsPercent(0);
                return;
            }

            Drivetrain drivetrain = PlayerCarManager.Instance.CurrentCar.drivetrain;
            float rpmAsPercent = Mathf.Clamp01(drivetrain.rpm / maxRpmDisplayed);
            UpdateNeedleAsPercent(rpmAsPercent);
        }

        private void UpdateNeedleAsPercent(float percent)
        {
            float difference = minMaxNeedleRotation.Max - minMaxNeedleRotation.Min;
            float desiredRotation = minMaxNeedleRotation.Max - (percent * difference);

            float rotationLerped = Mathf.LerpAngle(needle.eulerAngles.z, desiredRotation, Time.deltaTime * needleLerpSpeed);
            
            needle.eulerAngles = needle.eulerAngles.SetZ(rotationLerped);
            Debug.Log("Desired: " + Mathf.RoundToInt(desiredRotation) + " - Actual: " + Mathf.RoundToInt(rotationLerped));
        }
        
    }
}
