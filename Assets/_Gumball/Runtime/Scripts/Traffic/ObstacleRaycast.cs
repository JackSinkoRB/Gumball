using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class ObstacleRaycast
    {

        [SerializeField] private float offset;
        [SerializeField] private float raycastLength;
        [SerializeField] private float startDistance = 2;
        [SerializeField] private Vector3 detectorSize = new(0.7f,1,0.7f);
        [SerializeField] private LayerMask detectionLayers;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isBlocked;
        [SerializeField, ReadOnly] private float angle;
        [SerializeField, ReadOnly] private Vector3 offsetVector;

        private RaycastHit? blockage;
        
        public bool IsBlocked => isBlocked;
        public RaycastHit? Blockage => blockage;
        public float Angle => angle;
        public Vector3 OffsetVector => offsetVector;
        public float RaycastLength => raycastLength;
        
        private readonly RaycastHit[] blockagesTemp = new RaycastHit[10]; //is used for all blockage checks, not to be used for debugging

        public void DoRaycast(Transform transformFrom, Vector3 targetPosition, Vector3 targetDirection = default)
        {
            offsetVector = transformFrom.right * offset;
            
            Vector3 finalTargetPosition = targetPosition + offsetVector;
            Vector3 finalStartPosition = transformFrom.position + (transformFrom.forward * startDistance);
            Vector3 directionToTarget = Vector3.Normalize(finalTargetPosition - finalStartPosition);

            blockage = GetBlockage(transformFrom, directionToTarget);
            isBlocked = blockage != null;
            
            angle = Vector3.Angle(targetDirection == default ? transformFrom.forward : targetDirection, directionToTarget);
        }

        public void SetRaycastLength(float length)
        {
            raycastLength = length;
        }
        
        /// <summary>
        /// Gets the closest blockage in the given direction.
        /// </summary>
        private RaycastHit? GetBlockage(Transform transformFrom, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            
            Vector3 startPosition = transformFrom.position + (transformFrom.forward * startDistance);
            int hits = Physics.BoxCastNonAlloc(startPosition, detectorSize, direction, blockagesTemp, rotation, raycastLength, detectionLayers);
            RaycastHit? actualHit = null;
            
            RaycastHitSorter.SortRaycastHitsByDistance(blockagesTemp, hits);
            
            for (int index = 0; index < hits; index++)
            {
                RaycastHit hit = blockagesTemp[index];
                if (!ReferenceEquals(hit.transform.gameObject, transformFrom.gameObject))
                {
                    actualHit = hit;
                    break; //just get the first/closest hit
                }
            }
            
#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(startPosition, detectorSize, rotation, direction, raycastLength, actualHit != null ? Color.magenta : Color.gray);
#endif

            return actualHit;
        }
        
    }
}
