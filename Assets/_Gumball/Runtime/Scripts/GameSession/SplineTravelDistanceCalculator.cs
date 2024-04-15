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
        [Tooltip("The distance along the spline from the car's starting position to the start of the map. ")]
        [SerializeField, ReadOnly] private float initialSplineDistance;

        private int lastFrameCalculated = -1;
        
        private AICar car => GetComponent<AICar>();

        public float InitialDistance => initialSplineDistance;
        
        public float DistanceTraveled
        {
            get
            {
                //only calculate once per frame
                if (lastFrameCalculated != Time.frameCount)
                    distanceTraveled = GetSplineDistanceTraveled() - initialSplineDistance;
                
                return distanceTraveled;
            }
        }

        private void OnEnable()
        {
            initialSplineDistance = GetSplineDistanceTraveled();
        }
        
        private float GetSplineDistanceTraveled()
        {
            Chunk currentChunk = car.CurrentChunk;
            if (currentChunk == null)
                return 0;
            
            //get the distance in the current chunk
            Vector3 carPosition = car.transform.position; //TODO: use front of car
            int currentChunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(currentChunk);
            float distanceInCurrentChunk = currentChunk.GetDistanceTravelledAlongSpline(carPosition);
            
            //get the distance in previous chunks
            float distanceInPreviousChunks = 0;
            for (int index = 0; index < currentChunkIndex; index++)
            {
                ChunkMapData chunkData = ChunkManager.Instance.CurrentChunkMap.GetChunkData(index);
                distanceInPreviousChunks += chunkData.SplineLength;
            }

            return distanceInCurrentChunk + distanceInPreviousChunks;
        }
        
    }
}
