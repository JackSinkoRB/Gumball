using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
#if UNITY_EDITOR
using Gumball.Editor;
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
        
        private Collider collider => GetComponent<Collider>();

        public bool FlattenTerrain => flattenTerrain;
        public float FlattenTerrainRadius => flattenTerrainRadius;
        public float FlattenTerrainBlendRadius => flattenTerrainBlendRadius;
        
        public Vector3 GetLowestPosition() => collider != null
            ? collider.ClosestPoint(collider.bounds.center.OffsetY(-int.MaxValue))
            : transform.position;
        
        private void Initialise()
        {
            if (chunkBelongsTo == null)
                FindChunkBelongsTo();
        }
        
        private void OnEnable()
        {
            Initialise();

            SceneView.duringSceneGui += OnSceneUpdate;
            chunkBelongsTo.onTerrainChanged += OnTerrainChanged;
            
            UpdatePosition();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneUpdate;
            chunkBelongsTo.onTerrainChanged -= OnTerrainChanged;
        }
        
        private void OnValidate()
        {
            UpdatePosition();
        }

        private void OnSceneUpdate(SceneView sceneView)
        {
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
            Initialise();

            float offset = 0;

            Vector3 originalPosition = transform.position;
            transform.position = transform.position.SetY(chunkBelongsTo.CurrentTerrain.transform.position.y + 10000);
            
            if (Physics.Raycast(GetLowestPosition(), Vector3.down, out RaycastHit hitDown, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.Terrain)))
                offset = -hitDown.distance;

            if (offset == 0)
            {
                //wasn't successful
                transform.position = originalPosition;
            }
            else
            {
                //success
                transform.OffsetY(offset);
                
                lastKnownPositionWhenGrounded = transform.position;
            }
        }
        
        private void MoveToSpecificDistanceFromRoad()
        {
            Initialise();
            
            //get the closest spline
            var (closestSample, distanceToSplineSqr) = chunkBelongsTo.GetClosestSampleOnSpline(transform.position, true);
            
            Debug.DrawRay(closestSample.position, closestSample.right * distanceFromRoad, Color.red, 15);
            Vector3 desiredPosition = closestSample.position + (closestSample.right * distanceFromRoad);

            transform.position = desiredPosition.SetY(transform.position.y);
        }

        private void UpdatePosition()
        {
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
            Debug.LogError($"Could not find a chunk that {gameObject.name} belongs to.");
        }
        
#endif
    }
}
