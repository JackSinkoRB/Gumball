using System;
using System.Collections;
using System.Collections.Generic;
using PaintIn3D;
using UnityEngine;
using MyBox;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Gumball
{
    public class LiveDecal : MonoBehaviour
    {

        private const float selectionColliderWidth = 0.1f;
        
        [SerializeField] private Collider selectionCollider;
        [SerializeField] private P3dPaintDecal paintDecal;

        [Header("Settings")]
        [SerializeField] private LayerMask raycastLayers; //TODO - vehicle only
        [SerializeField] private MinMaxFloat minMaxScale = new(0.03f, 50);
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Sprite sprite;
        
        private Vector2 clickOffset;
        private Vector3 lastKnownPosition;
        private Quaternion lastKnownRotation;
        private int priority;
        private bool isClickableUnderPointerOnPress;

        public bool IsValidPosition { get; private set; }

        public P3dPaintDecal PaintDecal => paintDecal;
        public Sprite Sprite => sprite;
        public int Priority => priority;
        public Vector3 Scale => paintDecal.Scale;
        public float Angle => paintDecal.Angle;
        public Color Color => paintDecal.Color;

        private void SetDefaultPosition()
        {
            UpdatePosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
        }

        private void OnEnable()
        {
            SetScale(paintDecal.Scale.x);
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

        public void SetColor(Color color)
        {
            paintDecal.Color = color;
        }

        public void SetPriority(int priority)
        {
            this.priority = priority;
        }

        private void OnPrimaryContactPressed()
        {
            CalculateClickOffset();
            isClickableUnderPointerOnPress = PrimaryContactInput.IsClickableUnderPointer();
        }
        
        private void OnPrimaryContactPerformed()
        {
            if (DecalManager.Instance.CurrentSelected == this 
                && !isClickableUnderPointerOnPress)
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
                
                //put the selection collider on the angle of the normal
                selectionCollider.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);

                IsValidPosition = hit.collider.GetComponent<P3dPaintable>() != null;
            }
        }

        private void DrawPreview()
        {
            paintDecal.HandleHitPoint(true, int.MaxValue, 1, 0, lastKnownPosition, lastKnownRotation);
        }

        public void SetAngle(float angle)
        {
            paintDecal.Angle = angle;
        }
        
        public void SetScale(float scale)
        {
            float newValue = Mathf.Clamp(scale, minMaxScale.Min, minMaxScale.Max);
            Vector3 newScale = Vector3.one * newValue;
            
            paintDecal.Scale = newScale;
            
            //set the selection collider scale:
            Vector3 selectionScale = (newScale * (2 * paintDecal.Radius)).SetZ(selectionColliderWidth);
            selectionCollider.transform.localScale = selectionScale;
        }
    }
}
