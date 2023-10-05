using System;
using System.Collections;
using System.Collections.Generic;
using PaintIn3D;
using UnityEngine;
using MyBox;
using UnityEngine.EventSystems;

namespace Gumball
{
    public class LiveDecal : MonoBehaviour
    {

        [SerializeField] private Collider selectionCollider;
        [SerializeField] private P3dPaintDecal paintDecal;
        [SerializeField] private LayerMask raycastLayers;

        [SerializeField, ReadOnly] private Sprite sprite;
        
        private Vector2 clickOffset;
        private Vector3 lastKnownPosition;
        private Quaternion lastKnownRotation;
        private int priority;

        public bool IsValidPosition { get; private set; }

        public P3dPaintDecal PaintDecal => paintDecal;
        public Sprite Sprite => sprite;
        public int Priority => priority;
        
        private void SetDefaultPosition()
        {
            UpdatePosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
        }

        private void OnEnable()
        {
            SetScale(paintDecal.Scale);
            SetDefaultPosition();
            PrimaryContactInput.onPress += OnPrimaryContactPressed;
            PrimaryContactInput.onPerform += OnPrimaryContactPerformed;
            PrimaryContactInput.onRelease += OnPrimaryContactReleased;
        }

        private void OnDisable()
        {
            PrimaryContactInput.onPress -= OnPrimaryContactPressed;
            PrimaryContactInput.onPerform -= OnPrimaryContactPerformed;
            PrimaryContactInput.onRelease -= OnPrimaryContactReleased;
        }

        private void Update()
        {
            DrawPreview();
        }
        
        /// <summary>
        /// Applies the current state of the live decal to the car's material.
        /// </summary>
        public void Apply()
        {
            paintDecal.HandleHitPoint(false, int.MaxValue, 1, 0, lastKnownPosition, lastKnownRotation);
        }
        
        public void SetSprite(Sprite sprite)
        {
            this.sprite = sprite;
        }

        public void SetPriority(int priority)
        {
            this.priority = priority;
        }

        private void OnPrimaryContactPressed()
        {
            CalculateClickOffset();
        }
        
        private void OnPrimaryContactPerformed()
        {
            if (DecalManager.Instance.CurrentSelected == this 
                && !PrimaryContactInput.IsSelectableUnderPointer())
            {
                UpdatePosition(PrimaryContactInput.Position - clickOffset);
            }
        }

        private void OnPrimaryContactReleased()
        {
            if (DecalManager.Instance.CurrentSelected == this &&
                !IsValidPosition)
            {
                Destroy(gameObject);
            }
        }

        private void CalculateClickOffset()
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            clickOffset = PrimaryContactInput.Position - screenPosition;
        }

        private void UpdatePosition(Vector2 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers))
            {
                lastKnownPosition = hit.point;
                lastKnownRotation = Quaternion.LookRotation(Camera.main.transform.forward - hit.normal, Camera.main.transform.up);

                transform.position = lastKnownPosition;

                IsValidPosition = hit.collider.GetComponent<P3dPaintable>() != null;
            }
        }

        private void DrawPreview()
        {
            paintDecal.HandleHitPoint(true, int.MaxValue, 1, 0, lastKnownPosition, lastKnownRotation);
        }

        public void SetScale(Vector3 scale)
        {
            paintDecal.Scale = scale;
            selectionCollider.transform.localScale = scale * 2 * paintDecal.Radius;
        }
    }
}
