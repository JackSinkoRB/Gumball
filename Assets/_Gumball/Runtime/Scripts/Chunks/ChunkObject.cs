using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// Add to a GameObject in a chunk to give extra options for how it behaves in the chunk.
    /// </summary>
    public class ChunkObject : MonoBehaviour
    {
#if UNITY_EDITOR
        
        [Tooltip("When enabled, the transform is always moved to be placed on the terrain.")]
        [SerializeField] private bool alwaysGrounded;

        [Space(5)]
        [Tooltip("When enabled, the terrain is flattened to the bottom of the chunk object.")]
        [SerializeField] private bool keepAtSpecificDistanceFromRoad;
        [Tooltip("When enabled, the transform is always moved to be placed on the terrain.")]
        [SerializeField, ConditionalField(nameof(keepAtSpecificDistanceFromRoad)), PositiveValueOnly]
        private float distanceFromRoad = 10;
        
        [Space(5)]
        [Tooltip("When enabled, the terrain is flattened to the bottom of the chunk object.")]
        [SerializeField] private bool flattenTerrain;
        [Tooltip("The distance around the transform that is flattened.")]
        [SerializeField, ConditionalField(nameof(flattenTerrain)), PositiveValueOnly]
        private float flattenTerrainRadius = 5;
        [Tooltip("The distance after the flattening for the terrain to be blended with it's original height.")]
        [SerializeField, ConditionalField(nameof(flattenTerrain)), PositiveValueOnly]
        private float flattenTerrainBlendDistance = 5;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Chunk chunkBelongsTo;

        private Collider collider => GetComponent<Collider>();

        private void OnValidate()
        {
            if (chunkBelongsTo == null)
                FindChunkBelongsTo();
            
            if (alwaysGrounded)
                GroundObject();
        }
        
        /// <summary>
        /// Moves the transform so that the object is grounded on it's chunk.
        /// </summary>
        public void GroundObject()
        {
            float offset = 0;

            int terrainLayerMask = 1 << LayerMask.NameToLayer(ChunkUtils.TerrainLayer);

            transform.position = transform.position.SetY(chunkBelongsTo.CurrentTerrain.transform.position.y + 10000);
            
            bool useBottomOfCollider = collider != null;
            if (useBottomOfCollider)
            {
                Vector3 bottomOfCollider = collider.ClosestPoint(collider.bounds.center.OffsetY(-int.MaxValue));
                if (Physics.Raycast(bottomOfCollider, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, terrainLayerMask))
                    offset = -hitDown.distance;
            }
            else
            {
                //use the transform position
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, terrainLayerMask))
                    offset = -hitDown.distance;
            }
            
            transform.OffsetY(offset);
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
