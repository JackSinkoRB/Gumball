using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

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
        
        [Flags]
        public enum VehicleType
        {
            BIKE = 1 << 1,
            CAR = 1 << 2,
            TRUCK = 1 << 3
        }

        [SerializeField] private LaneType type;
        [ConditionalField(nameof(type), compareValues: nameof(LaneType.DISTANCE_FROM_CENTER)), SerializeField] private float distanceFromCenter;
        [ConditionalField(nameof(type), compareValues: nameof(LaneType.CUSTOM_SPLINE)), SerializeField] private CustomDrivingPath path;

        [Space(5)]
        [SerializeField, MultipleEnum] private VehicleType vehicleTypes;
        
        private List<VehicleType> selectedVehicleTypesCached;

        private List<VehicleType> selectedVehicleTypes
        {
            get
            {
                selectedVehicleTypesCached ??= MultipleEnumUtils.GetSelectedValues(vehicleTypes);

                if (selectedVehicleTypesCached.Count == 0)
                {
                    //add all by default - it might not have been setup
                    foreach (VehicleType vehicleType in Enum.GetValues(typeof(VehicleType)))
                        selectedVehicleTypesCached.Add(vehicleType);
                }

                return selectedVehicleTypesCached;
            }
        }

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
        
        public GameObject GetVehicleToSpawn()
        {
            GameSession session = GameSessionManager.Instance.CurrentSession;

            List<VehicleType> flaggedTypes = new List<VehicleType>();
            if (selectedVehicleTypes.Contains(VehicleType.BIKE) && session.TrafficBikes != null && session.TrafficBikes.Length > 0)
                flaggedTypes.Add(VehicleType.BIKE);
            if (selectedVehicleTypes.Contains(VehicleType.CAR) && session.TrafficCars != null && session.TrafficCars.Length > 0)
                flaggedTypes.Add(VehicleType.CAR);
            if (selectedVehicleTypes.Contains(VehicleType.TRUCK) && session.TrafficTrucks != null && session.TrafficTrucks.Length > 0)
                flaggedTypes.Add(VehicleType.TRUCK);

            if (flaggedTypes.Count == 0)
            {
                Debug.LogError($"Could not find car to spawn because the game session ({session.name}) has no vehicles of the desired type.");
                return null;
            }

            VehicleType randomType = flaggedTypes[Random.Range(0, flaggedTypes.Count)];

            return randomType switch
            {
                VehicleType.BIKE => session.GetTrafficVehicleHandle(session.TrafficBikes.GetRandom()).Result as GameObject,
                VehicleType.CAR => session.GetTrafficVehicleHandle(session.TrafficCars.GetRandom()).Result as GameObject,
                VehicleType.TRUCK => session.GetTrafficVehicleHandle(session.TrafficTrucks.GetRandom()).Result as GameObject,
                _ => null
            };
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
