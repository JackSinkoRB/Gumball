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
        [SerializeField, ReadOnly] private Vector3 targetOffset;
        [SerializeField] private Vector3 defaultTargetOffset = new(0, 0.5f);
        [SerializeField] private Vector3 decalTargetOffset = new(0, -1f);

        [Header("Movement")]
        [SerializeField] private float xSpeed = 0.5f;
        [SerializeField] private float ySpeed = 0.5f;
        [SerializeField] private MinMaxFloat yClamp = new(10, 60);
        [SerializeField] private float decelerationDuration = 0.5f;
        [SerializeField] private float decelerationSpeed = 50;
        [SerializeField] private float movementTweenDuration = 0.3f;
        
        [Header("Zoom")]
        [SerializeField] private float distance = 5;
        [SerializeField] private float pinchZoomSpeed = 1;
        [SerializeField] private float keyboardZoomSpeed = 1;
        [SerializeField] private MinMaxFloat zoomDistanceClamp = new(10, 50);
        
        private float horizontal;
        private float vertical;
        private Vector2 velocity;
        private Sequence currentMovementTween;
        private Tween decelerationTween;
        private bool pressedUI;

        private void OnEnable()
        {
            DecalEditor.onSessionStart += OnSessionStart;
            DecalEditor.onSessionEnd += OnSessionEnd;
        }
        
        private void OnDisable()
        {
            DecalEditor.onSessionStart -= OnSessionStart;
            DecalEditor.onSessionEnd -= OnSessionEnd;
        }

        private void OnSessionStart()
        {
            PrimaryContactInput.onPress += OnPrimaryContactPress;
            PrimaryContactInput.onDrag += OnPrimaryContactMove;
            PrimaryContactInput.onRelease += OnPrimaryContactRelease;
            PinchInput.onPinch += OnPinch;
            
            SetTarget(DecalEditor.Instance.CurrentCar.transform, defaultTargetOffset);

            DecalEditor.Instance.onSelectLiveDecal += OnSelectDecal;
            DecalEditor.Instance.onDeselectLiveDecal += OnDeselectDecal;
        }

        private void OnSessionEnd()
        {
            DecalEditor.Instance.onSelectLiveDecal -= OnSelectDecal;
            DecalEditor.Instance.onDeselectLiveDecal -= OnDeselectDecal;

            PrimaryContactInput.onPress -= OnPrimaryContactPress;
            PrimaryContactInput.onDrag -= OnPrimaryContactMove;
            PrimaryContactInput.onRelease -= OnPrimaryContactRelease;
            PinchInput.onPinch -= OnPinch;
            
            decelerationTween?.Kill();
            
            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            CheckToZoomWithKeyboard();
        }

        private void LateUpdate()
        {
            if (PrimaryContactInput.IsPressed)
                SetVelocity(PrimaryContactInput.OffsetSinceLastFrame);
        }

        public void SetTarget(Transform target, Vector2 offset)
        {
            this.target = target;
            targetOffset = offset;
            MoveCamera(Vector2.zero, movementTweenDuration);
        }
        
        private void OnPinch(Vector2 offset)
        {
            ModifyZoom((offset.x + offset.y) * pinchZoomSpeed);
        }

        private void OnPrimaryContactPress()
        {
            velocity = Vector2.zero;
            horizontal = Camera.main.transform.eulerAngles.y;
            vertical = Camera.main.transform.eulerAngles.x;

            pressedUI = PrimaryContactInput.IsGraphicUnderPointer();
        }
        
        private void OnPrimaryContactRelease()
        {
            CheckToDecelerate();
            CheckIfSelectedDecalMoved();
        }

        private void OnPrimaryContactMove(Vector2 offset)
        {
            if (!PrimaryContactInput.IsPressed)
                return;

            if (pressedUI)
                return; //don't move the camera if selecting UI

            if (DecalEditor.Instance.CurrentSelected != null && DecalEditor.Instance.CurrentSelected.WasUnderPointerOnPress)
                return;
            
            if (PinchInput.IsPinching)
            {
                SetVelocity(Vector2.zero);
                return;
            }

            SetVelocity(offset);
            MoveCamera(velocity);
        }

        private void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }

        private void MoveCamera(Vector2 offset, float duration = 0)
        {
            if (target == null)
                return;

            float actualDistance;
            if (target == PlayerCarManager.Instance.CurrentCar.transform)
            {
                actualDistance = distance;
            }
            else
            {
                //keep the camera at the same zoomed amount
                //actualDistance = distance - (what the distance would be from the camera to the car, minus the distance from the camera to the target)
                Vector3 carTargetPos = PlayerCarManager.Instance.CurrentCar.transform.position + defaultTargetOffset;
                Vector3 targetPos = target.transform.position + targetOffset;
                float distanceFromCameraToCar = Vector3.Distance(Camera.main.transform.position, carTargetPos);
                Vector3 heightDifference = new Vector3(0, carTargetPos.y - targetPos.y, 0);
                float distanceFromCameraToTarget = Vector3.Distance(Camera.main.transform.position, targetPos - heightDifference);
                actualDistance = distance - (distanceFromCameraToCar - distanceFromCameraToTarget);
            }

            horizontal += offset.x * xSpeed * actualDistance;
            vertical -= offset.y * ySpeed;

            vertical = ClampAngle(vertical, yClamp.Min, yClamp.Max);

            Vector3 rotationEuler = new Vector3(vertical, horizontal, 0);
            Quaternion rotation = Quaternion.Euler(rotationEuler);
            Vector3 position = rotation * new Vector3(0, 0, -actualDistance) + target.position + targetOffset;

            currentMovementTween?.Kill();
            if (duration == 0)
            {
                Camera.main.transform.rotation = rotation;
                Camera.main.transform.position = position;
            }
            else
            {
                decelerationTween?.Kill();
                currentMovementTween = DOTween.Sequence()
                    .Join(Camera.main.transform.DOMove(position, duration))
                    .Join(Camera.main.transform.DORotate(rotationEuler, duration));
            }
        }
        
        private void ModifyZoom(float value)
        {
            float newDistance = distance - value;
            newDistance = Mathf.Clamp(newDistance, zoomDistanceClamp.Min, zoomDistanceClamp.Max);
            
            distance = newDistance;
            MoveCamera(Vector2.zero, 0.1f);
        }

        private void DoDecelerationTween()
        {
            decelerationTween?.Kill();
            decelerationTween = DOTween.To(() => velocity, x => velocity = x, Vector2.zero, decelerationDuration)
                .OnUpdate(() => MoveCamera(velocity * Time.deltaTime * decelerationSpeed));
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
        
        private void OnSelectDecal(LiveDecal selectedDecal)
        {
            SetTarget(selectedDecal.transform, decalTargetOffset);
        }
        
        private void OnDeselectDecal(LiveDecal decalDeselected)
        {
            SetTarget(DecalEditor.Instance.CurrentCar.transform, defaultTargetOffset);
        }
        
        private void CheckToDecelerate()
        {
            if (pressedUI)
                return;

            if (currentMovementTween != null && currentMovementTween.IsPlaying())
                return;
            
            if (DecalEditor.Instance.CurrentSelected != null && !DecalEditor.Instance.CurrentSelected.WasUnderPointerOnPress)
                return;

            if (PinchInput.IsPinching)
                return;
            
            DoDecelerationTween();
        }

        private void CheckIfSelectedDecalMoved()
        {
            if (DecalEditor.Instance.CurrentSelected == null || !DecalEditor.Instance.CurrentSelected.WasUnderPointerOnPress)
                return;

            if (PinchInput.IsPinching)
                return;

            MoveCamera(Vector2.zero, movementTweenDuration);
        }
        
        /// <summary>
        /// Alternative for PC zooming (plus and minus keys).
        /// </summary>
        private void CheckToZoomWithKeyboard()
        {
#if UNITY_EDITOR || !UNITY_ANDROID
            if (Keyboard.current.numpadPlusKey.isPressed)
            {
                ModifyZoom(keyboardZoomSpeed);
            }
            else if (Keyboard.current.numpadMinusKey.isPressed)
            {
                ModifyZoom(-keyboardZoomSpeed);
            }
#endif
        }
        
    }
}
