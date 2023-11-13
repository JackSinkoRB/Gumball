using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gumball
{
    public class DecalCameraController : MonoBehaviour
    {
        
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new(0, 0.5f);
        [SerializeField] private float distance = 5.0f;
        [SerializeField] private float xSpeed = 0.5f;
        [SerializeField] private float ySpeed = 0.5f;
        [SerializeField] private MinMaxFloat yClamp = new(10, 60);
        [SerializeField] private float decelerationDuration = 0.5f;
        [SerializeField] private float zoomSpeed = 1;

        private float horizontal;
        private float vertical;
        private Vector2 velocity;
        
        private Tween decelerationTween;

        private void OnEnable()
        {
            PrimaryContactInput.onPress += OnPrimaryContactPress;
            PrimaryContactInput.onDrag += OnPrimaryContactMove;
            PrimaryContactInput.onRelease += OnPrimaryContactRelease;
            PinchInput.onPinch += OnPinch;

            target = PlayerCarManager.Instance.CurrentCar.transform;
        }

        private void OnDisable()
        {
            PrimaryContactInput.onPress -= OnPrimaryContactPress;
            PrimaryContactInput.onDrag -= OnPrimaryContactMove;
            PrimaryContactInput.onRelease -= OnPrimaryContactRelease;
            PinchInput.onPinch -= OnPinch;
            
            decelerationTween?.Kill();
        }
        
        private void Update()
        {
            CheckToZoomWithKeyboard();
        }
        
        private void OnPinch(Vector2 offset)
        {
            ModifyZoom(offset.x + offset.y);
        }

        private void OnPrimaryContactPress()
        {
            velocity = Vector2.zero;
            horizontal = Camera.main.transform.eulerAngles.y;
            vertical = Camera.main.transform.eulerAngles.x;

            pressedUI = PrimaryContactInput.IsClickableUnderPointer();
        }

        private bool pressedUI;

        private void OnPrimaryContactRelease()
        {
            DoDecelerationTween();
        }
        
        private void OnPrimaryContactMove(Vector2 offset)
        {
            if (DecalEditor.Instance.CurrentSelected != null)
                return;
            
            if (!PrimaryContactInput.IsPressed)
                return;

            if (pressedUI)
                return; //don't move the camera if selecting UI
            
            velocity = offset;
            MoveCamera(offset);
        }

        private void MoveCamera(Vector2 offset)
        {
            horizontal += offset.x * xSpeed * distance;
            vertical -= offset.y * ySpeed;

            vertical = ClampAngle(vertical, yClamp.Min, yClamp.Max);

            Quaternion rotation = Quaternion.Euler(vertical, horizontal, 0);
            Vector3 position = rotation * new Vector3(0, 0, -distance) + target.position + targetOffset;

            Camera.main.transform.rotation = rotation;
            Camera.main.transform.position = position;
        }

        private void ModifyZoom(float value)
        {
            float newDistance = distance - (Time.deltaTime * value * zoomSpeed);
            distance = newDistance;
            MoveCamera(velocity);
        }
        
        private void DoDecelerationTween()
        {
            decelerationTween?.Kill();
            decelerationTween = DOTween.To(() => velocity, x => velocity = x, Vector2.zero, decelerationDuration)
                .OnUpdate(() => MoveCamera(velocity));
        }
        
        private float ClampAngle(float angle, float min, float max)
        {
            float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
            float hdtAngle = dtAngle * 0.5f;
            float midAngle = min + hdtAngle;
 
            float offset = Mathf.Abs(Mathf.DeltaAngle(angle, midAngle)) - hdtAngle;
            if (offset > 0)
                angle = Mathf.MoveTowardsAngle(angle, midAngle, offset);
            return angle;
        }
        
        /// <summary>
        /// Alternative for PC zooming (plus and minus keys).
        /// </summary>
        private void CheckToZoomWithKeyboard()
        {
#if UNITY_EDITOR || !UNITY_ANDROID
            const float keyboardZoomSpeed = 800f;
            if (Keyboard.current.numpadPlusKey.isPressed)
            {
                ModifyZoom(keyboardZoomSpeed * Time.deltaTime);
            }
            else if (Keyboard.current.numpadMinusKey.isPressed)
            {
                ModifyZoom(-keyboardZoomSpeed * Time.deltaTime);
            }
#endif
        }
        
    }
}
