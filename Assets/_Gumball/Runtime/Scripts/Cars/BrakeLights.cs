using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gumball
{
    public class BrakeLights : MonoBehaviour
    {

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        
        [SerializeField] private float intensityWithHeadlightsOn = 0.2f;
        [SerializeField] private float intensityWhenBrakingWithHeadlightsOn = 1f;
        [SerializeField] private float intensityWhenBrakingWithHeadlightsOff = 0.4f;
        
        [SerializeField] private float intensityTweenDuration = 0.05f;
        [SerializeField] private float brakeFlareTweenDuration = 0.1f;
        [SerializeField] private float brakeFlareMaxOpacity = 0.35f;

        [SerializeField] private SpriteRenderer brakelightL;
        [SerializeField] private SpriteRenderer brakelightR;
        [SerializeField] private SpriteRenderer nightlightL;
        [SerializeField] private SpriteRenderer nightlightR;
        [Space(5)]
        [Tooltip("Add any mesh renderers that should have emission when the brakes or nightlights are applied.")]
        [SerializeField] private MeshRenderer[] meshRenderersForEmission;
        
        private Material materialInstance;

        private Sequence brakeFlareTween;
        private Tween intensityTween;
        
        private float desiredIntensity;
        private float currentIntensity;

        private bool showBrakes = true; //starts showing
        
        private void LateUpdate()
        {
            BillboardToCamera();
        }

        private void BillboardToCamera()
        {
            brakelightL.transform.LookAt(brakelightL.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            brakelightR.transform.LookAt(brakelightR.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            nightlightL.transform.LookAt(nightlightL.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up); 
            nightlightR.transform.LookAt(nightlightR.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
        
        public void CheckToEnable(AICar car)
        {
            if (car == null)
                return;
            
            bool sessionActive = GameSessionManager.ExistsRuntime
                                 && GameSessionManager.Instance.CurrentSession != null;
            
            //get emission intensity
            float intensity = 0;
            if (sessionActive)
            {
                if (GameSessionManager.Instance.CurrentSession.EnableCarHeadlights)
                    intensity = car.IsBraking ? intensityWhenBrakingWithHeadlightsOn : intensityWithHeadlightsOn;
                else if (car.IsBraking)
                    intensity = intensityWhenBrakingWithHeadlightsOff;
            }
            
            //set the intensity
            if (!intensity.Approximately(desiredIntensity))
            {
                desiredIntensity = intensity;
                
                //start the tween
                intensityTween?.Kill();
                intensityTween = DOTween.To(() => currentIntensity, x => currentIntensity = x, intensity, intensityTweenDuration)
                    .OnUpdate(() => SetIntensity(currentIntensity));
            }

            //do brake flare tween
            bool shouldShowBrakes = sessionActive && car.IsBraking;

            if (showBrakes != shouldShowBrakes)
            {
                brakeFlareTween?.Kill();
                brakeFlareTween = DOTween.Sequence()
                    .Join(brakelightL.DOFade(shouldShowBrakes ? brakeFlareMaxOpacity : 0, brakeFlareTweenDuration))
                    .Join(brakelightR.DOFade(shouldShowBrakes ? brakeFlareMaxOpacity : 0, brakeFlareTweenDuration));
            }
            
            showBrakes = shouldShowBrakes;
            
            //do night lights
            nightlightL.gameObject.SetActive(sessionActive && GameSessionManager.Instance.CurrentSession.EnableCarHeadlights);
            nightlightR.gameObject.SetActive(sessionActive && GameSessionManager.Instance.CurrentSession.EnableCarHeadlights);
        }

        private void SetIntensity(float intensity)
        {
            foreach (MeshRenderer meshRenderer in meshRenderersForEmission)
            {
                if (materialInstance == null)
                    materialInstance = Instantiate(meshRenderer.material);
                
                if (meshRenderer.material != materialInstance)
                    meshRenderer.material = materialInstance;
                
                materialInstance.SetColor(EmissionColor, new Color(intensity, 0, 0, 1));
            }
        }
        
    }
}
