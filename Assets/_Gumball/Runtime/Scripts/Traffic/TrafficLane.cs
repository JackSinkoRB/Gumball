using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class TrafficLane
    {

        public enum LaneType
        {
            DISTANCE_FROM_CENTER,
            CUSTOM_SPLINE
        }

        [SerializeField] private LaneType type;
        [ConditionalField(nameof(type), compareValues: nameof(LaneType.DISTANCE_FROM_CENTER)), SerializeField] private float distanceFromCenter;
        [ConditionalField(nameof(type), compareValues: nameof(LaneType.CUSTOM_SPLINE)), SerializeField] private CustomDrivingPath path;

        public LaneType Type => type;
        public float DistanceFromCenter => distanceFromCenter;
        public CustomDrivingPath Path => path;

        public TrafficLane(float distanceFromCenter)
        {
            type = LaneType.DISTANCE_FROM_CENTER;
            this.distanceFromCenter = distanceFromCenter;
        }
        
        public TrafficLane(CustomDrivingPath path)
        {
            type = LaneType.CUSTOM_SPLINE;
            this.path = path;
        }

        public (bool, PositionAndRotation) CanMoveCarToLane(AICar playerCar, ChunkTrafficManager.LaneDirection laneDirection)
        {
            ChunkTrafficManager trafficManager = playerCar.LastKnownChunk.TrafficManager;
            
            switch (type)
            {
                case LaneType.DISTANCE_FROM_CENTER:
                {
                    var (closestSample, closestDistanceSqr) = playerCar.LastKnownChunk.GetClosestSampleOnSpline(playerCar.transform.TransformPoint(playerCar.FrontOfCarPosition));

                    PositionAndRotation lanePosition = trafficManager.GetLanePosition(closestSample, distanceFromCenter, laneDirection);
                    bool spaceIsFree = trafficManager.CanSpawnCarAtPosition(playerCar, closestSample.position, closestSample.rotation, true);
                    return (spaceIsFree, lanePosition);
                }
                case LaneType.CUSTOM_SPLINE:
                {
                    var (closestSample, closestDistanceSqr) = path.SampleCollection.GetClosestSampleOnSpline(playerCar.transform.TransformPoint(playerCar.FrontOfCarPosition));

                    PositionAndRotation lanePosition = trafficManager.GetLanePosition(closestSample, 0, laneDirection);
                    bool spaceIsFree = trafficManager.CanSpawnCarAtPosition(playerCar, closestSample.position, closestSample.rotation, true);
                    return (spaceIsFree, lanePosition);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float GetDistanceFromCenter(AICar playerCar)
        {
            //if a path,

            if (type == LaneType.DISTANCE_FROM_CENTER)
                return distanceFromCenter;

            if (type == LaneType.CUSTOM_SPLINE)
            {
                //get the closest sample on the path to the player
                var (closestSample, closestSampleDistance) = path.SampleCollection.GetClosestSampleOnSpline(playerCar.transform.position);
                //then get the distance to the closest sample on the chunk spline
                var (closestSampleOnChunk, closestSampleDistanceOnChunk) = playerCar.LastKnownChunk.GetClosestSampleOnSpline(closestSample.position);
                return closestSampleDistanceOnChunk;
            }

            throw new NotImplementedException();
        }
    }
}
