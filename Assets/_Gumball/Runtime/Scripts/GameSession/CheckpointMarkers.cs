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
            
        public void Spawn(GameSession session, float distanceAlongSpline)
        {
            SplineSample sampleAlongSplines = session.GetSampleAlongSplines(distanceAlongSpline);
            
            //spawn the marker on both sides
            Vector3 leftPosition = sampleAlongSplines.position + -sampleAlongSplines.right * distanceFromSpline;
            Vector3 rightPosition = sampleAlongSplines.position + sampleAlongSplines.right * distanceFromSpline;

            GameObject left = checkpointMarkerPrefab.GetSpareOrCreate(position: leftPosition, rotation: sampleAlongSplines.rotation);
            GameObject right = checkpointMarkerPrefab.GetSpareOrCreate(position: rightPosition, rotation: sampleAlongSplines.rotation);
                
            ChunkUtils.GroundObject(left.transform);
            ChunkUtils.GroundObject(right.transform);
        }
        
    }
}
