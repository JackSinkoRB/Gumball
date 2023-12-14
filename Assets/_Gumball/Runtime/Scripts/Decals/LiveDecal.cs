using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PaintIn3D;
using UnityEngine;
using MyBox;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Gumball
{
    public class LiveDecal : MonoBehaviour
    {

        [Serializable]
        public struct LiveDecalData
        {
            [SerializeField] private int categoryIndex;
            [SerializeField] private int textureIndex;
            [SerializeField] private int priority;
            [SerializeField] private SerializedVector3 lastKnownPosition;
            [SerializeField] private SerializedVector3 lastKnownRotationEuler;
            [SerializeField] private SerializedVector3 lastKnownHitNormal;
            [SerializeField] private SerializedVector3 scale;
            [SerializeField] private float angle;
            [SerializeField] private int colorIndex;

            public int CategoryIndex => categoryIndex;
            public int TextureIndex => textureIndex;
            public int Priority => priority;
            public SerializedVector3 LastKnownPosition => lastKnownPosition;
            public SerializedVector3 LastKnownRotationEuler => lastKnownRotationEuler;
            public SerializedVector3 LastKnownHitNormal => lastKnownHitNormal;
            
            public SerializedVector3 Scale => scale;
            public float Angle => angle;
            public int ColorIndex => colorIndex;
            
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
                colorIndex = liveDecal.ColorIndex;
            }
        }

        public event Action<Color, Color> onColorChanged;
        public event Action onMoved;
        
        /// <summary>
        /// The default colour index to use for decals that can be coloured.
        /// </summary>
        private const int defaultColourIndex = 4;
        private const float selectionColliderWidth = 0.1f;
        
        [SerializeField] private Collider selectionCollider;
        [SerializeField] private P3dPaintDecal paintDecal;

        [Header("Settings")]
        [SerializeField] private LayerMask raycastLayers; //TODO - vehicle only
        [SerializeField] private MinMaxVector3 minMaxScale = new(0.1f * Vector3.one, 3.5f * Vector3.one);

        [Header("Debugging")]
        [SerializeField, ReadOnly] private int priority;
        [SerializeField, ReadOnly] private int categoryIndex;
        [SerializeField, ReadOnly] private int textureIndex;
        [SerializeField, ReadOnly] private Sprite sprite;
        [SerializeField, ReadOnly] private int colorIndex = -1;
        
        private DecalTexture textureData;
        private Vector2 clickOffset;
        private Vector3 lastKnownPosition;
        private Quaternion lastKnownRotation;
        private Vector3 lastKnownHitNormal;
        private bool wasClickableUnderPointerOnPress;
        private DecalStateManager.ModifyStateChange stateBeforeMoving;
        private DecalStateManager.DestroyStateChange stateBeforeDestroying;

        public bool IsValidPosition { get; private set; }
        
        public P3dPaintDecal PaintDecal => paintDecal;
        public Sprite Sprite => sprite;
        public int Priority => priority;
        public Vector3 Scale => paintDecal.Scale;
        public float Angle => paintDecal.Angle;
        public Color Color => paintDecal.Color;
        public bool WasUnderPointerOnPress { get; private set; }
        public DecalTexture TextureData => textureData;
        public int ColorIndex => colorIndex;
        
        /// <summary>
        /// Force the decal to be valid.
        /// </summary>
        public void SetValid()
        {
            IsValidPosition = true;
        }
        
        public void UpdatePosition(Vector3 position, Vector3 hitNormal, Quaternion rotation)
        {
            bool hasMoved = !lastKnownPosition.Approximately(position, PrimaryContactInput.DragThreshold);

            lastKnownPosition = position;
            lastKnownRotation = rotation;
            lastKnownHitNormal = hitNormal;
            
            transform.position = lastKnownPosition;

            if (hasMoved)
                onMoved?.Invoke();
            
            //put the selection collider on the angle of the normal
            selectionCollider.transform.rotation = Quaternion.LookRotation(hitNormal, Vector3.up);
        }

        private void OnEnable()
        {
            SetScale(paintDecal.Scale);
            SetDefaultPosition();
        }

        private void OnDisable()
        {
            colorIndex = -1; //reset if reused in pool
        }

        private void LateUpdate()
        {
            if (IsValidPosition)
                DrawPreview();
        }

        public void Initialise(DecalTexture textureData, int categoryIndex, int textureIndex)
        {
            this.textureData = textureData;
            this.categoryIndex = categoryIndex;
            this.textureIndex = textureIndex;
            
            SetSprite(textureData.Sprite);
            
            if (textureData.CanColour && colorIndex == -1)
                colorIndex = defaultColourIndex;
            SetColor(textureData.CanColour ? DecalEditor.Instance.ColorPalette[colorIndex] : Color.white);
            
            SetScale(Vector3.one);
            SetAngle(0);
            SetValid();
        }

        public void OnSelect()
        {
            PrimaryContactInput.onPress += OnPrimaryContactPressed;
            PrimaryContactInput.onPerform += OnPrimaryContactPerformed;
            PrimaryContactInput.onRelease += OnPrimaryContactReleased;
        }

        public void OnDeselect()
        {
            PrimaryContactInput.onPress -= OnPrimaryContactPressed;
            PrimaryContactInput.onPerform -= OnPrimaryContactPerformed;
            PrimaryContactInput.onRelease -= OnPrimaryContactReleased;
        }
        
        /// <summary>
        /// Applies the data to the live decal.
        /// </summary>
        public void PopulateWithData(LiveDecalData data)
        {
            if (textureData.CanColour)
                SetColorFromIndex(data.ColorIndex);
            else SetColor(Color.white);
            
            UpdatePosition(data.LastKnownPosition.ToVector3(), data.LastKnownHitNormal.ToVector3(), Quaternion.Euler(data.LastKnownRotationEuler.ToVector3()));
            SetScale(data.Scale.ToVector3());
            SetAngle(data.Angle);
            SetValid();
        }
        
        /// <summary>
        /// Applies the current state of the live decal to the car's material.
        /// </summary>
        public void Apply()
        {
            paintDecal.HandleHitPoint(false, priority, 1, 0, lastKnownPosition, lastKnownRotation);
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

        private void SetColor(Color color)
        {
            Color previousColor = paintDecal.Color;
            paintDecal.Color = color;
            
            onColorChanged?.Invoke(previousColor, color);
        }
        
        /// <summary>
        /// Set the color using the index from the color palette.
        /// </summary>
        public void SetColorFromIndex(int colorIndex)
        {
            this.colorIndex = colorIndex;
            SetColor(DecalEditor.Instance.ColorPalette[colorIndex]);
        }

        public void SetPriority(int priority)
        {
            this.priority = priority;
        }
        
        /// <summary>
        /// Flip the priority of the current selected decal and the next priority decal.
        /// </summary>
        public void SendBackwardOrForward(bool isForward, List<LiveDecal> overlappingDecals)
        {
            List<LiveDecal> decalsSorted = overlappingDecals;
            decalsSorted.Add(this);
            decalsSorted = decalsSorted.OrderBy(liveDecal => liveDecal.Priority).ToList();

            int currentPriority = priority;
            int currentIndex = decalsSorted.IndexOf(this);
            LiveDecal nextDecal = decalsSorted[currentIndex + (isForward ? 1 : -1)];
            int nextPriority = nextDecal.Priority;
            
            //flip the priorities
            SetPriority(nextPriority);
            nextDecal.SetPriority(currentPriority);
            
            //priorities have changed, make sure to reorder the list
            DecalEditor.Instance.OrderDecalsListByPriority();
        }

        private void OnPrimaryContactPressed()
        {
            CalculateClickOffset();

            Graphic[] excludeRing = {DecalEditor.Instance.SelectedDecalUI.Ring};
            wasClickableUnderPointerOnPress = PrimaryContactInput.IsGraphicUnderPointer(excludeRing);
            
            float maxRaycastDistance = Vector3.Distance(Camera.main.transform.position, PlayerCarManager.Instance.CurrentCar.transform.position);
            WasUnderPointerOnPress = PrimaryContactInput.IsGraphicUnderPointer(DecalEditor.Instance.SelectedDecalUI.Ring) //check if the ring is first, as it is cached
                                     || PrimaryContactInput.IsColliderUnderPointer(selectionCollider, maxRaycastDistance, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.LiveDecal));

            stateBeforeMoving = new DecalStateManager.ModifyStateChange(this);
            stateBeforeDestroying = new DecalStateManager.DestroyStateChange(this);
        }
        
        private void OnPrimaryContactPerformed()
        {
            if (DecalEditor.Instance.CurrentSelected != this)
                return;

            if (wasClickableUnderPointerOnPress)
                return;

            bool pointerWasDragged = !PrimaryContactInput.OffsetSincePressedNormalised.Approximately(Vector2.zero, PrimaryContactInput.DragThreshold);
            if (!pointerWasDragged)
                return;
            
            if (WasUnderPointerOnPress)
                OnMoveScreenPosition(PrimaryContactInput.Position - clickOffset);
        }

        private void OnPrimaryContactReleased()
        {
            if (DecalEditor.Instance.CurrentSelected != this)
                return;

            if (!WasUnderPointerOnPress)
                return;
            
            if (!IsValidPosition)
            {
                DecalStateManager.LogStateChange(stateBeforeDestroying);
                DecalEditor.Instance.DisableLiveDecal(this);
            }
            else
            {
                bool positionHasMoved = !transform.position.Approximately(stateBeforeMoving.Data.LastKnownPosition.ToVector3(), PrimaryContactInput.DragThreshold);
                if (positionHasMoved)
                    DecalStateManager.LogStateChange(stateBeforeMoving);
            }
        }

        public List<LiveDecal> GetOverlappingLiveDecals()
        {
            List<LiveDecal> overlappingDecals = new List<LiveDecal>();
            
            foreach (LiveDecal liveDecal in DecalEditor.Instance.LiveDecals)
            {
                if (liveDecal == this)
                    continue;
                
                BoxCollider boxCollider = (BoxCollider) liveDecal.selectionCollider;
                
                if (boxCollider.bounds.Intersects(selectionCollider.bounds))
                    overlappingDecals.Add(liveDecal);
            }

            return overlappingDecals;
        }

        private void SetDefaultPosition()
        {
            OnMoveScreenPosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
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
                Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.forward - hit.normal, Vector3.up);
                UpdatePosition(hit.point, hit.normal, rotation);
                
                //always facing camera:
                //UpdatePosition(hit.point, hit.normal, Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up));

                //just using hit:
                //UpdatePosition(hit.point, hit.normal, Quaternion.LookRotation(-hit.normal, Vector3.up));
                
                //using camera and hit:
                //UpdatePosition(hit.point, hit.normal, Quaternion.LookRotation(Camera.main.transform.forward - hit.normal, Camera.main.transform.up));

                IsValidPosition = hit.collider.GetComponent<P3dPaintable>() != null;
            }
            else
            {
                //TODO: update position?
                IsValidPosition = false;
            }
        }

        private void DrawPreview()
        {
            paintDecal.HandleHitPoint(true, priority, 1, 0, lastKnownPosition, lastKnownRotation);
        }
        
    }
}
