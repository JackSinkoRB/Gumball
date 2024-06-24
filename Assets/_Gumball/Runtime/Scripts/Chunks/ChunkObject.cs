using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// Add to a GameObject in a chunk to give extra options for how it behaves in the chunk.
    /// </summary>
    [ExecuteAlways]
    public class ChunkObject : MonoBehaviour
    {
        
        [Header("Setup check")]
        [HelpBox("The object is not a valid prefab asset (ending in .prefab). ChunkObject can only be added to prefabs. Therefore this object will not show at runtime.", MessageType.Error, true)]
        [SerializeField, ReadOnly] private bool isPrefab;
        [HelpBox("This ChunkObject will not function properly because it is a child of another ChunkObject.", MessageType.Error, true, true)]
        [SerializeField, ReadOnly] private bool isChildOfAnotherChunkObject;
        
        [Header("Settings")]
        [Tooltip("Should the object be ignored from the chunk at runtime? eg. if it is just to modify the terrain etc.")]
        [SerializeField] private bool ignoreAtRuntime;
        
        [Tooltip("If enabled, the chunk will ignore these objects and load them separately across multiple frames to reduce instantiation lag.")]
        [HelpBox("The object is not loading separately, which can contribute to lag when the chunk is loaded.", MessageType.Warning, true)]
        [SerializeField, ConditionalField(nameof(ignoreAtRuntime), true)] private bool loadSeparately = true;
        
        [Space(10)]
        [Tooltip("When enabled, the transform is always moved to be placed on the terrain.")]
        [SerializeField] private bool alwaysGrounded;
        
        [Space(10)]
        [Tooltip("When enabled, the terrain is flattened to the bottom of the chunk object.")]
        [SerializeField] private bool keepAtSpecificDistanceFromRoad;
        [Tooltip("When enabled, the transform is always moved to be X distance away from the road spline.")]
        [SerializeField, ConditionalField(nameof(keepAtSpecificDistanceFromRoad))] private float distanceFromRoad = 10;
        
        [Space(10)]
        [Tooltip("When enabled, the terrain is flattened to the collider specified below.")]
        [HelpBox("Must manually recreate the terrain to apply this setting. Use the 'Recreate Terrain' button below.", MessageType.Info, true, true)]
        [SerializeField] private bool flattenTerrain;
        [Tooltip("Assign a collider for the terrain to flatten to.")]
        [SerializeField, ConditionalField(nameof(flattenTerrain))]
        private Collider colliderToFlattenTo;
        [Tooltip("The distance after the flattening for the terrain to be blended with it's original height.")]
        [SerializeField, ConditionalField(nameof(flattenTerrain)), PositiveValueOnly]
        private float flattenTerrainBlendDistance = 20;
        [SerializeField, ConditionalField(nameof(flattenTerrain))]
        private AnimationCurve flattenTerrainBlendCurve = AnimationCurve.Linear(0, 1, 1, 0);
        
        [Space(10)]
        [Tooltip("When enabled, the specified vertex colour is blended into the current vertex colours around the supplied mesh.")]
        [HelpBox("Must manually recreate the terrain to apply this setting. Use the 'Recreate Terrain' button below.", MessageType.Info, true, true)]
        [SerializeField] private bool colourTerrain;
        [SerializeField, ConditionalField(nameof(colourTerrain))] private ChunkObjectColorModifier colorModifier;

        public ChunkObjectColorModifier ColorModifier => colorModifier;
        public bool CanColourTerrain => isActiveAndEnabled && colourTerrain && colorModifier.ColliderToColourAround != null;

        public bool CanFlattenTerrain => isActiveAndEnabled && flattenTerrain && colliderToFlattenTo != null;
        public Collider ColliderToFlattenTo => colliderToFlattenTo;
        public float FlattenTerrainBlendDistance => flattenTerrainBlendDistance;
        public AnimationCurve FlattenTerrainBlendCurve => flattenTerrainBlendCurve;
        
        public bool LoadSeparately => loadSeparately;
        public bool IgnoreAtRuntime => ignoreAtRuntime;
        public bool AlwaysGrounded => alwaysGrounded;
        public Chunk Chunk => chunkBelongsTo;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunkBelongsTo;
        
        [SerializeField, HideInInspector] private Vector3 lastKnownPositionWhenGrounded;
        
#if UNITY_EDITOR
        [ButtonMethod]
        public void RecreateTerrain()
        {
            chunkBelongsTo.GetComponent<ChunkEditorTools>().RecreateTerrain();
        }
        
        public bool IsChildOfAnotherChunkObject {
            get
            {
                Transform parent = transform.parent;
                while (parent != null)
                {
                    if (parent.GetComponent<ChunkObject>() != null)
                        return true;
                    parent = parent.parent;
                }

                return false;
            }
        }

        public void SetChunkBelongsTo(Chunk chunk)
        {
            chunkBelongsTo = chunk;
        }

        private void Initialise()
        {
            //unsubscribe if already subscribed
            chunkBelongsTo.onTerrainChanged -= OnTerrainChanged;
            chunkBelongsTo.onTerrainChanged += OnTerrainChanged;
            
            UpdatePosition();
        }

        private void OnEnable()
        {
            TryInitialise();
            if (!Application.isPlaying)
                SceneView.duringSceneGui += OnSceneUpdate;
        }

        private void TryInitialise()
        {
            if (chunkBelongsTo != null)
                return;
            
            chunkBelongsTo = transform.GetComponentInAllParents<Chunk>();
            if (chunkBelongsTo == null)
                return;
            
            //good to initialise
            Initialise();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneUpdate;

            if (chunkBelongsTo != null)
                chunkBelongsTo.onTerrainChanged -= OnTerrainChanged;
        }
        
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdatePosition();
                CheckToSetColliderToFlattenTo();

                isPrefab = GameObjectUtils.GetPathToPrefabAsset(gameObject) != null;
                isChildOfAnotherChunkObject = IsChildOfAnotherChunkObject;
            }
        }

        private void CheckToSetColliderToFlattenTo()
        {
            //if there is no collider assigned, assign the current collider
            if (flattenTerrain
                && colliderToFlattenTo == null
                && GetComponent<Collider>() != null)
            {
                colliderToFlattenTo = GetComponent<Collider>();
            }
        }

        private void OnSceneUpdate(SceneView sceneView)
        {
            if (Application.isPlaying)
                return;
            
            Vector3 currentPosition = transform.position;
            if (currentPosition.Approximately(lastKnownPositionWhenGrounded))
                return;

            UpdatePosition();
        }

        private void OnTerrainChanged()
        {
            UpdatePosition();
        }

        /// <summary>
        /// Moves the transform so that the object is grounded on it's chunk.
        /// </summary>
        public void GroundObject()
        {
            ChunkUtils.GroundObject(transform);
            lastKnownPositionWhenGrounded = transform.position;
        }
        
        private void MoveToSpecificDistanceFromRoad()
        {
            //get the closest spline
            var (closestSample, distanceToSplineSqr) = chunkBelongsTo.GetClosestSampleOnSpline(transform.position);
            
            Debug.DrawRay(closestSample.position, closestSample.right * distanceFromRoad, Color.red, 15);
            Vector3 desiredPosition = closestSample.position + (closestSample.right * distanceFromRoad);

            transform.position = desiredPosition.SetY(transform.position.y);
        }

        public void UpdatePosition()
        {
            if (!gameObject.scene.IsValid())
                return;
            
            TryInitialise();

            if (chunkBelongsTo == null)
                return;
            
            if (keepAtSpecificDistanceFromRoad)
                MoveToSpecificDistanceFromRoad();
            
            if (alwaysGrounded)
                GroundObject();
        }
        
#endif
    }
}
