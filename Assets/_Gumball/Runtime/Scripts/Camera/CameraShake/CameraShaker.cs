using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CameraShaker : Singleton<CameraShaker>
    {
        
        private const float maxRotShake = 1f;
        private const float maxPosShake = .3f;
        
        [Header("Debugging")]
        [ReadOnly, SerializeField] private List<CameraShakeInstance> cameraShakeInstances = new();
        
        public ReadOnlyCollection<CameraShakeInstance> ShakeInstances => cameraShakeInstances.AsReadOnly();

        private void LateUpdate()
        {
            Vector3 posAddShake = Vector3.zero;
            Vector3 rotAddShake = Vector3.zero;

            for (int i = 0; i < cameraShakeInstances.Count; i++)
            {
                if (i >= cameraShakeInstances.Count)
                    break;

                CameraShakeInstance shake = cameraShakeInstances[i];
                
                if (shake.CurrentState == CameraShakeInstance.State.Inactive)
                {
                    Debug.Log("Deactivate shake");
                    cameraShakeInstances.RemoveAt(i);
                    i--;
                }
                else if (shake.CurrentState != CameraShakeInstance.State.Inactive)
                {
                    Vector3 shakeAmount = shake.UpdateShake();
                    posAddShake += shakeAmount.Multiply(shake.PositionInfluence);
                    rotAddShake += shakeAmount.Multiply(shake.RotationInfluence);
                    Debug.Log($"Update shake : {shakeAmount}");
                }
            }

            //TODO: don't clamp - it will cause it to stay at the edges - need to handle another way
            //clamp to max
            rotAddShake = rotAddShake.ClampValues(-maxRotShake, maxRotShake);
            posAddShake = posAddShake.ClampValues(-maxPosShake, maxPosShake);
            
            transform.localPosition = posAddShake;
            transform.localEulerAngles = rotAddShake;
        }

        /// <summary>
        /// Starts a shake.
        /// </summary>
        /// <returns>A CameraShakeInstance that can be used to alter the shake's properties.</returns>
        public CameraShakeInstance DoShake(CameraShakeInstance shake)
        {
            cameraShakeInstances.Add(shake);
            shake.StartFadeIn();
            return shake;
        }
        
    }
}
