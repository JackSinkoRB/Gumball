using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public readonly struct LaneData
    {
            
        public float LaneOffset { get; }
        public CustomDrivingPath CustomLane { get; }
        public float RandomLaneDistance { get; }
        public PositionAndRotation LanePosition { get; }
            
        public bool IsCustomLane => CustomLane != null;
            
        public LaneData(float laneOffset, CustomDrivingPath customLane, float randomLaneDistance, PositionAndRotation lanePosition)
        {
            LaneOffset = laneOffset;
            CustomLane = customLane;
            RandomLaneDistance = randomLaneDistance;
            LanePosition = lanePosition;
        }
            
    }
}
