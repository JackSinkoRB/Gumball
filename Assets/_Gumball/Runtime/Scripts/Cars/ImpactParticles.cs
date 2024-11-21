using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class ImpactParticles : MonoBehaviour
    {
        
        [SerializeField] private Light impactLight;
        [SerializeField] private AnimationCurve impactBrightness;
        [SerializeField] private float lightDuration = 0.2f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float intensityMultiplier = 5f;

        private float elapsedTime;
        
        private void OnEnable()
        {
            elapsedTime = 0f;
        }
        
        private void LateUpdate()
        {
            elapsedTime += Time.deltaTime;
            impactLight.intensity = impactBrightness.Evaluate(Mathf.Clamp01(elapsedTime / lightDuration)) * intensityMultiplier;
            
            if (elapsedTime > duration)
                gameObject.Pool();
        }
        
    }
}