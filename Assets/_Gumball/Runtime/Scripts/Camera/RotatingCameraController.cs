using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gumball
{
    /// <summary>
    /// A camera controller that can rotate around a target object.
    /// </summary>
    public class RotatingCameraController : MonoBehaviour
    {
        
        [SerializeField] private Vector2 initialCameraOffset;
        
        [Header("Target")]
        [SerializeField, ReadOnly] private Transform target;
        [SerializeField, ReadOnly] private Vector3 targetOffset;
        [SerializeField] protected Vector3 defaultTargetOffset = new(0, 0.5f);
        
        [Header("Movement")]
        [SerializeField] private float xSpeed = 50;
        [SerializeField] private float ySpeed = 100;
        [SerializeField] private MinMaxFloat yClamp = new(10, 60);
        [SerializeField] private float decelerationDuration = 0.5f;
        [SerializeField] private float decelerationSpeed = 50;
        [SerializeField] private float movementTweenDuration = 0.3f;
        
        [Header("Zoom")]
        [SerializeField] private float distance = 5;
        [SerializeField] private float pinchZoomSpeed = 0.01f;
        [SerializeField] private float keyboardZoomSpeed = 10;
        [SerializeField] private MinMaxFloat zoomDistanceClamp = new(3, 6);

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Vector2 totalOffset;
        
        private float horizontal;
        private float vertical;
        private Vector2 velocity;
        private Sequence currentMovementTween;
        private Tween decelerationTween;
        private bool pressedUI;
        
        protected virtual void OnEnable()
        {
            PrimaryContactInput.onPress += OnPrimaryContactPress;
            PrimaryContactInput.onDrag += OnPrimaryContactMove;
            PrimaryContactInput.onRelease += OnPrimaryContactRelease;
            PinchInput.onPinch += OnPinch;
        }
        
        protected virtual void OnDisable()
        {
            PrimaryContactInput.onPress -= OnPrimaryContactPress;
            PrimaryContactInput.onDrag -= OnPrimaryContactMove;
            PrimaryContactInput.onRelease -= OnPrimaryContactRelease;
            PinchInput.onPinch -= OnPinch;
            
            decelerationTween?.Kill();
            currentMovementTween?.Kill();
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
            if (Camera.main == null)
                return; //scene might not be set up yet
            
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
            
            bool positionHasMoved = !PrimaryContactInput.OffsetSincePressedNormalised.Approximately(Vector2.zero, PrimaryContactInput.PressedThreshold);
            if (!positionHasMoved)
                return;
            
            SetVelocity(offset);
            MoveCamera(velocity);
        }

        private void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }
        
        protected void SetInitialPosition()
        {
            decelerationTween?.Kill();
            currentMovementTween?.Kill();
            MoveCamera(initialCameraOffset);
        }

        private void MoveCamera(Vector2 offset, float duration = 0)
        {
            if (target == null)
                return;

            totalOffset += offset;
            
            float actualDistance;
            if (target == WarehouseManager.Instance.CurrentCar.transform)
            {
                actualDistance = distance;
            }
            else
            {
                //keep the camera at the same zoomed amount
                float distanceFromCarToTarget = Vector3.Distance(
                    WarehouseManager.Instance.CurrentCar.transform.position + defaultTargetOffset, 
                    target.transform.position + targetOffset);
                actualDistance = distance - distanceFromCarToTarget;
            }

            horizontal += offset.x * xSpeed * actualDistance; //multiply by distance so the closer you are, the slower it rotates
            vertical -= offset.y * ySpeed;

            vertical = ClampAngle(vertical, yClamp.Min, yClamp.Max);
            
            Vector3 rotationEuler = new Vector3(vertical, horizontal, 0);
            Quaternion rotation = Quaternion.Euler(rotationEuler);
            Vector3 position = rotation * new Vector3(0, 0, -actualDistance) + target.position + targetOffset;

            currentMovementTween?.Kill();
            if (duration > 0)
                decelerationTween?.Kill();
            
            currentMovementTween = DOTween.Sequence()
                .Join(Camera.main.transform.DOMove(position, duration))
                .Join(Camera.main.transform.DORotate(rotationEuler, duration));
            
            if (duration == 0)
                currentMovementTween.Complete();
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

        private void CheckToDecelerate()
        {
            if (pressedUI)
                return;

            if (currentMovementTween != null && currentMovementTween.IsActive() && currentMovementTween.IsPlaying())
                return;
            
            if (DecalEditor.Instance.CurrentSelected != null && !DecalEditor.Instance.CurrentSelected.WasUnderPointerOnPress)
                return;

            if (PinchInput.IsPinching)
                return;
            
            bool positionHasMoved = !PrimaryContactInput.OffsetSincePressedNormalised.Approximately(Vector2.zero, PrimaryContactInput.PressedThreshold);
            if (!positionHasMoved)
                return;
            
            DoDecelerationTween();
        }

        private void CheckIfSelectedDecalMoved()
        {
            if (DecalEditor.Instance.CurrentSelected == null || !DecalEditor.Instance.CurrentSelected.WasUnderPointerOnPress)
                return;

            if (PinchInput.IsPinching)
                return;
            
            bool positionHasMoved = !PrimaryContactInput.OffsetSincePressedNormalised.Approximately(Vector2.zero, PrimaryContactInput.PressedThreshold);
            if (!positionHasMoved)
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
