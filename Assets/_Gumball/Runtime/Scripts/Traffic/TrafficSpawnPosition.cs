using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    [Serializable]
    public class TrafficSpawnPosition
    {
        
        [SerializeField] private float distanceFromMapStart;
        [SerializeField] private ChunkTrafficManager.LaneDirection laneDirection;
        [SerializeField] private int laneIndex;

        public float DistanceFromMapStart => distanceFromMapStart;
        public ChunkTrafficManager.LaneDirection LaneDirection => laneDirection;
        public int LaneIndex => laneIndex;

        public TrafficSpawnPosition(float distanceFromMapStart, ChunkTrafficManager.LaneDirection laneDirection, int laneIndex)
        {
            this.distanceFromMapStart = distanceFromMapStart;
            this.laneDirection = laneDirection;
            this.laneIndex = laneIndex;
        }

    }
}
