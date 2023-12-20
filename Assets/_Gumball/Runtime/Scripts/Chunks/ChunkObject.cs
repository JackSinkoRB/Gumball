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
#if UNITY_EDITOR

        [Tooltip("Should the object be ignored from the chunk at runtime? eg. if it is just to modify the terrain etc.")]
        [SerializeField] private bool ignoreAtRuntime;
        
        [Tooltip("If enabled, the chunk will ignore these objects and load them separately across multiple frames to reduce instantiation lag.")]
        [SerializeField, ConditionalField(nameof(ignoreAtRuntime), true)] private bool loadSeparately = true;
        
        [Space(10)]
        [Tooltip("When enabled, the transform is always moved to be placed on the terrain.")]
        [SerializeField] private bool alwaysGrounded;
        
        [Space(10)]
        [Tooltip("When enabled, the terrain is flattened to the bottom of the chunk object.")]
        [SerializeField] private bool keepAtSpecificDistanceFromRoad;
        [Tooltip("When enabled, the transform is always moved to be placed on the terrain.")]
        [SerializeField, ConditionalField(nameof(keepAtSpecificDistanceFromRoad))] private float distanceFromRoad = 10;
        
        [Space(10)]
        [Tooltip("When enabled, the terrain is flattened to the bottom of the chunk object.")]
        [HelpBox("Must manually recreate the terrain to apply this setting. Use the 'Recreate Terrain' button below.", MessageType.Info, true, true)]
        [SerializeField] private bool flattenTerrain;
        [Tooltip("The distance around the transform that is flattened.")]
        [SerializeField, ConditionalField(nameof(flattenTerrain)), PositiveValueOnly]
        private float flattenTerrainRadius = 5;
        [Tooltip("The distance after the flattening for the terrain to be blended with it's original height.")]
        [SerializeField, ConditionalField(nameof(flattenTerrain)), PositiveValueOnly]
        private float flattenTerrainBlendRadius = 20;

        [ButtonMethod(ButtonMethodDrawOrder.AfterInspector, nameof(flattenTerrain))]
        public void RecreateTerrain()
        {
            chunkBelongsTo.GetComponent<ChunkEditorTools>().RecreateTerrain();
        }

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunkBelongsTo;
        
        [SerializeField, HideInInspector] private Vector3 lastKnownPositionWhenGrounded;
        
        public bool FlattenTerrain => flattenTerrain;
        public float FlattenTerrainRadius => flattenTerrainRadius;
        public float FlattenTerrainBlendRadius => flattenTerrainBlendRadius;
        public bool LoadSeparately => loadSeparately;
        public bool IgnoreAtRuntime => ignoreAtRuntime;
        public bool AlwaysGrounded => alwaysGrounded;
        
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
            
            FindChunkBelongsTo();
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
            var (closestSample, distanceToSplineSqr) = chunkBelongsTo.GetClosestSampleOnSpline(transform.position, true);
            
            Debug.DrawRay(closestSample.position, closestSample.right * distanceFromRoad, Color.red, 15);
            Vector3 desiredPosition = closestSample.position + (closestSample.right * distanceFromRoad);

            transform.position = desiredPosition.SetY(transform.position.y);
        }

        private void UpdatePosition()
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
        
        private void FindChunkBelongsTo()
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                Chunk chunk = parent.GetComponent<Chunk>();
                if (chunk != null)
                {
                    chunkBelongsTo = chunk;
                    return;
                }
                parent = parent.parent;
            }

            chunkBelongsTo = null;
            Debug.LogWarning($"Could not find a chunk that {gameObject.name} belongs to.");
        }
        
#endif
    }
}
