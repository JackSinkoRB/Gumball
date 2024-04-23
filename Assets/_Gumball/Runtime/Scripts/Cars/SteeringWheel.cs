using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SteeringWheel : MonoBehaviour
    {

        [SerializeField] private Vector3 maxRotationEulerLeft;
        [SerializeField] private Vector3 maxRotationEulerRight;

        [Header("Tween")]
        [SerializeField] private float steerTweenDuration = 1f;
        [SerializeField] private Ease steerTweenEase = Ease.InOutSine;
        
        [SerializeField, ReadOnly] private float currentSteerAmount;
        private Tween currentSteerTween;
        
        /// <param name="amount">A value from -1 to 1, where 0 is centered, -1 is full lock left, and 1 is full lock right</param>
        public void UpdateSteeringAmount(float amount)
        {
            if (amount.Approximately(currentSteerAmount))
                return;

            currentSteerAmount = amount;
            
            Vector3 desiredRotation = Vector3.Lerp(maxRotationEulerLeft, maxRotationEulerRight, (amount + 1) / 2);
            
            currentSteerTween?.Kill();
            currentSteerTween = transform.DOLocalRotate(desiredRotation, steerTweenDuration).SetEase(steerTweenEase);
        }
        
    }
}
