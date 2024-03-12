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

        public bool IsBlocked => isBlocked;
        public float Angle => angle;
        public Vector3 OffsetVector => offsetVector;
        public float RaycastLength => raycastLength;
        
        private readonly RaycastHit[] blockagesTemp = new RaycastHit[5]; //is used for all blockage checks, not to be used for debugging
        
        public void DoRaycast(Transform transformFrom, Vector3 targetPosition)
        {
            offsetVector = transformFrom.right * offset;
            Vector3 directionToTarget = Vector3.Normalize(targetPosition + offsetVector - transformFrom.position + (transformFrom.forward * startDistance));

            RaycastHit? blockage = GetBlockage(transformFrom, directionToTarget);
            isBlocked = blockage != null;

            angle = Vector3.Angle(transformFrom.forward, directionToTarget);
        }

        /// <summary>
        /// Gets the closest blockage in the given direction.
        /// </summary>
        private RaycastHit? GetBlockage(Transform transformFrom, Vector3 direction)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            
            int hits = Physics.BoxCastNonAlloc(transformFrom.position + (transformFrom.forward * startDistance), detectorSize, direction, blockagesTemp, rotation, raycastLength, detectionLayers);
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
            BoxCastUtils.DrawBoxCastBox(transformFrom.position + (transformFrom.forward * startDistance), detectorSize, rotation, direction, raycastLength, actualHit != null ? Color.magenta : Color.gray);
#endif

            return actualHit;
        }
        
    }
}
