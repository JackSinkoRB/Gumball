using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCar : AICar
    {
        

        private const float blockedPathDetectorDistance = 20;
        private const float desiredDistanceToBlockage = 5;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isPathBlocked;
        [SerializeField, ReadOnly] private float distanceToBlockage;

        private readonly RaycastHit[] blockingObjects = new RaycastHit[10];
        
        private bool faceForward => currentChunk.TrafficManager.GetLaneDirection(CurrentLaneDistance) == ChunkTrafficManager.LaneDirection.FORWARD;
        
        public float CurrentLaneDistance { get; private set; }

        public override void Initialise(Chunk currentChunk)
        {
            base.Initialise(currentChunk);
            
            TrafficCarSpawner.TrackCar(this);
            
            //spawn at max speed
            SetMaxSpeed();
            
            gameObject.layer = (int)LayersAndTags.Layer.TrafficCar;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (!isInitialised)
                return; //didn't get a chance to initialise
            
            TrafficCarSpawner.UntrackCar(this);
        }

        public void SetLaneDistance(float laneDistance)
        {
            CurrentLaneDistance = laneDistance;
        }

        /// <summary>
        /// Sets the car's speed to max speed, and stops accelerating or decelerating.
        /// </summary>
        private void SetMaxSpeed()
        {
            float newDesiredSpeed = currentChunk.TrafficManager.SpeedLimitKmh;
            OnChangeDesiredSpeed(newDesiredSpeed);
            ForceSetSpeed(newDesiredSpeed);
        }

        protected override void PreMoveChecks()
        {
            base.PreMoveChecks();
            
            isPathBlocked = CheckIfPathIsBlocked();
            if (isPathBlocked)
            {
                //if distance to blockage is greater than stopped distance, set to slow speed, else 0
                const float slowSpeed = 25;
                float decelerationSpeed = distanceToBlockage > desiredDistanceToBlockage ? Mathf.Clamp01(distanceToBlockage / blockedPathDetectorDistance) * slowSpeed : 0;
                OnChangeDesiredSpeed(decelerationSpeed);
            }
            else
            {
                float newDesiredSpeed = currentChunk.TrafficManager.SpeedLimitKmh;
                if (!newDesiredSpeed.Approximately(DesiredSpeed))
                {
                    OnChangeDesiredSpeed(newDesiredSpeed);
                }
            }
        }

        private bool CheckIfPathIsBlocked()
        {
            BoxCollider carCollider = GetComponent<BoxCollider>();
            Vector3 boxCastHalfExtents = carCollider.size / 2f;
            int hits = Physics.BoxCastNonAlloc(transform.TransformPoint(carCollider.center.OffsetZ(carCollider.size.z)), boxCastHalfExtents, transform.forward, blockingObjects, transform.rotation, blockedPathDetectorDistance, LayersAndTags.AICarCollisionLayers);

            RaycastHitSorter.SortRaycastHitsByDistance(blockingObjects, hits);

            Collider actualHit = null;
            
            //don't include the car's own collider
            for (int index = 0; index < hits; index++)
            {
                RaycastHit hit = blockingObjects[index];
                if (!ReferenceEquals(hit.transform.gameObject, gameObject.transform.gameObject))
                {
                    actualHit = hit.collider;
                    distanceToBlockage = hit.distance;
                    break;
                }
            }

#if UNITY_EDITOR
            BoxCastUtils.DrawBoxCastBox(transform.TransformPoint(carCollider.center.OffsetZ(carCollider.size.z)), boxCastHalfExtents, transform.rotation, transform.forward, blockedPathDetectorDistance, actualHit != null ? Color.magenta : Color.gray);
#endif
            
            return actualHit != null;
        }
        
        /// <summary>
        /// Get the next desired position and rotation relative to the sample on the next chunk's spline.
        /// </summary>
        /// <returns>The spline sample's position and rotation, or null if no more loaded chunks in the desired direction.</returns>
        protected override (Chunk, Vector3, Quaternion)? GetPositionAhead(float distance)
        {
            if (currentChunk == null)
                return null;
            
            if (currentChunk.TrafficManager == null)
            {
                Debug.LogWarning($"A traffic car is on the chunk {currentChunk.gameObject.name}, but it doesn't have a traffic manager.");
                return null;
            }

            (SplineSample, Chunk)? splineSampleAhead = GetSplineSampleAhead(distance);
            if (splineSampleAhead == null)
                return null; //no more chunks loaded
            
            var (position, rotation) = currentChunk.TrafficManager.GetLanePosition(splineSampleAhead.Value.Item1, CurrentLaneDistance);

            return (splineSampleAhead.Value.Item2, position, rotation);
        }

        /// <summary>
        /// Gets the spline sample that is 'distance' metres away from the closest sample.
        /// </summary>
        private (SplineSample, Chunk)? GetSplineSampleAhead(float desiredDistance)
        {
            if (currentChunk.TrafficManager == null)
                return null; //no traffic manager

            float desiredDistanceSqr = desiredDistance * desiredDistance;

            Chunk chunkToUse = currentChunk;
            int chunkIndex = ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunkToUse);
            
            bool isChunkLoaded = chunkIndex >= 0;
            if (!isChunkLoaded)
                return null; //current chunk isn't loaded
            
            //get the closest sample, then get the next, and next, until it is X distance away from the closest
            int closestSplineIndex = currentChunk.GetClosestSampleIndexOnSpline(transform.position).Item1;
            SplineSample closestSample = currentChunk.SplineSamples[closestSplineIndex];

            SplineSample? previousSample = null;
            float previousDistanceOffset = 0;
            int offset = faceForward ? 1 : -1;
            while (true)
            {
                int sampleIndex = closestSplineIndex + offset;
                
                //check if it goes past the current chunk
                if (sampleIndex >= chunkToUse.SplineSamples.Length || sampleIndex < 0)
                {
                    //get the next chunk
                    chunkIndex = faceForward ? chunkIndex + 1 : chunkIndex - 1;
                    
                    LoadedChunkData? loadedChunkData = ChunkManager.Instance.GetLoadedChunkDataByMapIndex(chunkIndex);
                    if (loadedChunkData == null)
                    {
                        //no more loaded chunks
                        return null;
                    }
                    
                    Chunk newChunk = loadedChunkData.Value.Chunk;
                    chunkToUse = newChunk;
                    if (newChunk.TrafficManager == null)
                        return null; //no traffic manager

                    //reset the values
                    previousSample = null;
                    closestSplineIndex = newChunk.GetClosestSampleIndexOnSpline(transform.position).Item1;
                    closestSample = newChunk.SplineSamples[closestSplineIndex];
                    offset = faceForward ? 1 : -1;
                    continue;
                }
                
                SplineSample sample = chunkToUse.SplineSamples[closestSplineIndex + offset];
                float distanceToSampleSqr = Vector3.SqrMagnitude(sample.position - closestSample.position);
                float distanceOffset = Mathf.Abs(desiredDistanceSqr - distanceToSampleSqr);
                
                bool isFurtherAway = previousSample != null && distanceOffset > previousDistanceOffset;
                if (isFurtherAway)
                    return (previousSample.Value, chunkToUse);
                
                previousDistanceOffset = distanceOffset;
                previousSample = sample;
                
                offset = faceForward ? offset + 1 : offset - 1;
            }
        }

    }
}
