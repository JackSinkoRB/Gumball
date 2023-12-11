using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SelectedDecalUI : MonoBehaviour
    {

        [SerializeField] private Image ring;
        [SerializeField] private Image invalidDecal;
        [Space(5)]
        [SerializeField] private Image image;
        [SerializeField] private Color validColor;
        [SerializeField] private Color invalidColor;
        
        [Header("Fade when modifying")]
        [SerializeField] private float fadeWhenModifying = 0.2f;

        [Header("Scale/Rotation handle")]
        [SerializeField] private ButtonEvents scaleRotationHandle;
        [SerializeField] private float scaleSpeed = 2;
        
        private LiveDecal selectedDecal => DecalEditor.Instance.CurrentSelected;

        public Image Ring => ring;
        public ButtonEvents ScaleRotationHandle => scaleRotationHandle;

        private Tween currentFadeTween;
        private bool isFaded;
        
        private float lastKnownRadius;
        private Vector2 lastClickPosition;
        
        private DecalStateManager.StateChange stateBeforePressing;
        
        private void OnEnable()
        {
            scaleRotationHandle.onPress += OnPressScaleRotationHandle;
            scaleRotationHandle.onDrag += OnDragScaleRotationHandle;
            scaleRotationHandle.onRelease += OnReleaseScaleRotationHandle;
        }

        private void OnDisable()
        {
            scaleRotationHandle.onDrag -= OnDragScaleRotationHandle;
            scaleRotationHandle.onPress -= OnPressScaleRotationHandle;
            scaleRotationHandle.onRelease -= OnReleaseScaleRotationHandle;
        }

        public void UpdatePosition()
        {
            if (selectedDecal == null)
            {
                if (isFaded)
                    Fade(false);
                
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);

            MovePosition();
        }
        
        public void Fade(bool fade)
        {
            if (isFaded == fade)
                return; //already faded
            
            isFaded = fade;
            this.GetComponent<CanvasGroup>(true).alpha = fade ? fadeWhenModifying : 1;
        }
        
        private void OnPressScaleRotationHandle()
        {
            lastClickPosition = PrimaryContactInput.Position;
            lastKnownRadius = GetDistanceToCentre(PrimaryContactInput.Position);
            stateBeforePressing = new DecalStateManager.ModifyStateChange(selectedDecal);
        }

        private void OnReleaseScaleRotationHandle()
        {
            CheckToLogChange();
        }

        private void CheckToLogChange()
        {
            if (stateBeforePressing == null)
                return;
            
            bool positionHasMoved = !PrimaryContactInput.OffsetSincePressed.Approximately(Vector2.zero, 0.001f);
            if (positionHasMoved)
                DecalStateManager.LogStateChange(stateBeforePressing);
        }
        
        private void OnDragScaleRotationHandle(Vector2 offset)
        {
            if (!scaleRotationHandle.IsPressingButton)
                return;
            
            UpdateScale();
            UpdateRotation();
        }

        private void UpdateScale()
        {
            //scale offset = radius from middle
            float newRadius = GetDistanceToCentre(PrimaryContactInput.Position);
            float radiusDelta = newRadius - lastKnownRadius;
            lastKnownRadius = newRadius;

            Vector3 scaleOffset = Vector3.one * (radiusDelta * scaleSpeed);
            Vector3 existingScale = selectedDecal.Scale;
            Vector3 newScale = existingScale + scaleOffset;
            selectedDecal.SetScale(newScale);
        }
        
        private void UpdateRotation()
        {
            //rotation = angle between click point to centre and new point to centre
            float angleDelta = -Vector2.SignedAngle(PrimaryContactInput.Position - (Vector2)transform.position, lastClickPosition - (Vector2)transform.position);
            lastClickPosition = PrimaryContactInput.Position;

            float newAngle = selectedDecal.Angle + angleDelta;
            selectedDecal.SetAngle(newAngle);
        }

        private void MovePosition()
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(selectedDecal.transform.position);
            transform.position = screenPos;
            CheckIfValidPosition();
        }

        private void CheckIfValidPosition()
        {
            image.color = selectedDecal.IsValidPosition ? validColor : invalidColor;
            
            invalidDecal.gameObject.SetActive(!selectedDecal.IsValidPosition);
            if (!selectedDecal.IsValidPosition)
                invalidDecal.sprite = selectedDecal.Sprite;
        }

        private float GetDistanceToCentre(Vector2 fromPos)
        {
            return Vector2.Distance(fromPos, (Vector2)transform.position);
        }
        
    }
}
