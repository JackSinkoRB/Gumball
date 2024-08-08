using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CheckpointMarkers
    {

        [SerializeField] private GameObject checkpointMarkerPrefab;
        [SerializeField] private float distanceFromSpline = 5;
            
        public void Spawn(float distanceAlongSpline)
        {
            if (checkpointMarkerPrefab == null)
            {
                Debug.LogWarning("Cannot spawn checkpoint marker as there is no prefab assigned.");
                return;
            }
            
            SplineSample sampleAlongSplines = ChunkManager.Instance.GetSampleAlongSplines(distanceAlongSpline);
            
            //spawn the marker on both sides
            Vector3 leftPosition = sampleAlongSplines.position + -sampleAlongSplines.right * distanceFromSpline;
            Vector3 rightPosition = sampleAlongSplines.position + sampleAlongSplines.right * distanceFromSpline;

            GameObject left = checkpointMarkerPrefab.GetSpareOrCreate(position: leftPosition, rotation: sampleAlongSplines.rotation);
            GameObject right = checkpointMarkerPrefab.GetSpareOrCreate(position: rightPosition, rotation: sampleAlongSplines.rotation);
            
            ChunkUtils.GroundObject(left.transform);
            ChunkUtils.GroundObject(right.transform);

            //disable colliders after grounding
            Collider leftCollider = left.GetComponent<Collider>();
            if (leftCollider != null)
                leftCollider.enabled = false;
            Collider rightCollider = right.GetComponent<Collider>();
            if (rightCollider != null)
                rightCollider.enabled = false;
        }
        
    }
}
