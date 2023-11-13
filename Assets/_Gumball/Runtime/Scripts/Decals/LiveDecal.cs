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

        [Serializable]
        public class LiveDecalData
        {
            [SerializeField] private int categoryIndex;
            [SerializeField] private int textureIndex;
            [SerializeField] private int priority;
            [SerializeField] private SerializedVector3 lastKnownPosition;
            [SerializeField] private SerializedVector3 lastKnownRotationEuler;
            [SerializeField] private SerializedVector3 lastKnownHitNormal;
            [SerializeField] private SerializedVector3 scale;
            [SerializeField] private float angle;

            public int CategoryIndex => categoryIndex;
            public int TextureIndex => textureIndex;
            public int Priority => priority;
            public SerializedVector3 LastKnownPosition => lastKnownPosition;
            public SerializedVector3 LastKnownRotationEuler => lastKnownRotationEuler;
            public SerializedVector3 LastKnownHitNormal => lastKnownHitNormal;

            public SerializedVector3 Scale => scale;
            public float Angle => angle;

            public LiveDecalData(LiveDecal liveDecal)
            {
                categoryIndex = liveDecal.categoryIndex;
                textureIndex = liveDecal.textureIndex;
                priority = liveDecal.priority;
                lastKnownPosition = liveDecal.lastKnownPosition.ToSerializedVector();
                lastKnownRotationEuler = liveDecal.lastKnownRotation.eulerAngles.ToSerializedVector();
                lastKnownHitNormal = liveDecal.lastKnownHitNormal.ToSerializedVector();
                scale = liveDecal.Scale.ToSerializedVector();
                angle = liveDecal.Angle;
            }
        }
        
        private const float selectionColliderWidth = 0.1f;
        
        [SerializeField] private Collider selectionCollider;
        [SerializeField] private P3dPaintDecal paintDecal;

        [Header("Settings")]
        [SerializeField] private LayerMask raycastLayers; //TODO - vehicle only
        [SerializeField] private MinMaxVector3 minMaxScale = new(0.1f * Vector3.one, 3.5f * Vector3.one);

        [Header("Debugging")]
        [SerializeField, ReadOnly] private int categoryIndex;
        [SerializeField, ReadOnly] private int textureIndex;
        [SerializeField, ReadOnly] private Sprite sprite;
        
        private Vector2 clickOffset;
        private Vector3 lastKnownPosition;
        private Quaternion lastKnownRotation;
        private Vector3 lastKnownHitNormal;
        private int priority;
        private bool isClickableUnderPointerOnPress;

        public bool IsValidPosition { get; private set; }
        
        public P3dPaintDecal PaintDecal => paintDecal;
        public Sprite Sprite => sprite;
        public int Priority => priority;
        public Vector3 Scale => paintDecal.Scale;
        public float Angle => paintDecal.Angle;
        public Color Color => paintDecal.Color;

        /// <summary>
        /// Force the decal to be valid.
        /// </summary>
        public void SetValid()
        {
            IsValidPosition = true;
        }
        
        public void UpdatePosition(Vector3 position, Vector3 hitNormal, Quaternion rotation)
        {
            lastKnownPosition = position;
            lastKnownRotation = rotation;
            lastKnownHitNormal = hitNormal;
            
            transform.position = lastKnownPosition;
                
            //put the selection collider on the angle of the normal
            selectionCollider.transform.rotation = Quaternion.LookRotation(hitNormal, Vector3.up);
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

        public void Initialise(int categoryIndex, int textureIndex)
        {
            this.categoryIndex = categoryIndex;
            this.textureIndex = textureIndex;
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
            paintDecal.Texture = sprite.texture;
        }
        
        public void SetAngle(float angle)
        {
            paintDecal.Angle = angle;
        }
        
        public void SetScale(Vector3 scale)
        {
            Vector3 clampedScale = minMaxScale.Clamp(scale);
            
            paintDecal.Scale = clampedScale;
            
            //set the selection collider scale:
            Vector3 selectionScale = (clampedScale * (2 * paintDecal.Radius)).SetZ(selectionColliderWidth);
            selectionCollider.transform.localScale = selectionScale;
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
            if (DecalEditor.Instance.CurrentSelected == this 
                && !isClickableUnderPointerOnPress)
            {
                OnMoveScreenPosition(PrimaryContactInput.Position - clickOffset);
            }
        }

        private void OnPrimaryContactReleased()
        {
            if (DecalEditor.Instance.CurrentSelected == this &&
                !IsValidPosition)
            {
                RemoveDecal();
            }
        }

        private void SetDefaultPosition()
        {
            OnMoveScreenPosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
        }
        
        private void RemoveDecal()
        {
            DecalEditor.Instance.DestroyLiveDecal(this);
        }

        private void CalculateClickOffset()
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            clickOffset = PrimaryContactInput.Position - screenPosition;
        }

        private void OnMoveScreenPosition(Vector2 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers))
            {
                Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.forward - hit.normal, Camera.main.transform.up);
                UpdatePosition(hit.point, hit.normal, rotation);
                
                IsValidPosition = hit.collider.GetComponent<P3dPaintable>() != null;
            }
        }

        private void DrawPreview()
        {
            paintDecal.HandleHitPoint(true, priority, 1, 0, lastKnownPosition, lastKnownRotation);
        }
        
    }
}
