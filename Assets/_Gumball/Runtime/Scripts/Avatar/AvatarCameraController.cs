using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarCameraController : MonoBehaviour
    {

        [SerializeField] private Vector3 initialCameraPosition;
        [SerializeField] private Vector3 initialCameraRotation;
        
        [Header("Target")]
        [SerializeField, ReadOnly] private Transform target;
        [SerializeField, ReadOnly] private Vector3 targetOffset;
        [SerializeField] private Vector3 defaultTargetOffset = new(0, 0.5f);
        
        private void OnEnable()
        {
            AvatarEditor.onSessionStart += OnSessionStart;
            AvatarEditor.onSessionEnd += OnSessionEnd;
        }
        
        private void OnDisable()
        {
            AvatarEditor.onSessionStart -= OnSessionStart;
            AvatarEditor.onSessionEnd -= OnSessionEnd;
        }
        
        public void SetTarget(Transform target, Vector2 offset)
        {
            this.target = target;
            targetOffset = offset;
            //MoveCamera(Vector2.zero, movementTweenDuration);
        }

        private void OnSessionStart()
        {
            SetTarget(AvatarEditor.Instance.CurrentSelectedAvatar.transform, defaultTargetOffset);
            SetInitialPosition();
        }

        private void OnSessionEnd()
        {
            
        }
        
        private void SetInitialPosition()
        {
            Camera.main.transform.position = initialCameraPosition;
            Camera.main.transform.rotation = Quaternion.Euler(initialCameraRotation);
        }
        
    }
}
