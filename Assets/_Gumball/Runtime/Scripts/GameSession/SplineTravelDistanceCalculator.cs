using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SplineTravelDistanceCalculator : MonoBehaviour
    {

        [SerializeField, ReadOnly] private float distanceTraveled;
        [SerializeField, ReadOnly] private float distanceInMap;
        [Tooltip("The distance along the spline from the car's starting position to the start of the map. ")]
        [SerializeField, ReadOnly] private float initialDistance;

        private int lastFrameCalculated = -1;
        
        private AICar car => GetComponent<AICar>();
        
        public float DistanceInMap
        {
            get
            {
                UpdateCache();
                return distanceInMap;
            }
        }
        
        public float DistanceTraveled
        {
            get
            {
                UpdateCache();
                return distanceTraveled;
            }
        }

        private void OnEnable()
        {
            initialDistance = GetSplineDistanceTraveled();
        }

        private void UpdateCache()
        {
            if (lastFrameCalculated == Time.frameCount)
                return;
            
            lastFrameCalculated = Time.frameCount;
            
            distanceInMap = GetSplineDistanceTraveled();
            distanceTraveled = distanceInMap - initialDistance;
        }
        
        private float GetSplineDistanceTraveled()
        {
            Chunk currentChunk = car.CurrentChunk;
            if (currentChunk == null)
                return 0;
            
            //get the distance in the current chunk
            Vector3 carPosition = car.transform.TransformPoint(car.FrontOfCarPosition);
            int currentChunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(currentChunk);
            float distanceInCurrentChunk = currentChunk.GetDistanceTravelledAlongSpline(carPosition);
            
            //get the distance in previous chunks
            int previousChunkIndex = currentChunkIndex - 1;
            float distanceInPreviousChunks = previousChunkIndex < 0 ? 0 : ChunkManager.Instance.CurrentChunkMap.ChunkLengthsCalculated[previousChunkIndex];
            
            return distanceInCurrentChunk + distanceInPreviousChunks;
        }
        
    }
}
