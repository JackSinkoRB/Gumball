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
        [HelpBox("Optional: Select a specific car prefab variant to spawn.\nIf not selected, it will choose a random car from the lanes car collection.", MessageType.Info, HelpBoxAttribute.Position.ABOVE)]
        [SerializeField] private AICar carPrefab;

        public float DistanceFromMapStart => distanceFromMapStart;
        public ChunkTrafficManager.LaneDirection LaneDirection => laneDirection;
        public int LaneIndex => laneIndex;
        public AICar CarPrefab => carPrefab;

        public TrafficSpawnPosition(float distanceFromMapStart, ChunkTrafficManager.LaneDirection laneDirection, int laneIndex)
        {
            this.distanceFromMapStart = distanceFromMapStart;
            this.laneDirection = laneDirection;
            this.laneIndex = laneIndex;
        }

    }
}
