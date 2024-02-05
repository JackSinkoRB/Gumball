using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class TrafficCarSpawner : Singleton<TrafficCarSpawner>
    {

#if UNITY_EDITOR
        private int trafficCarID; //a unique identifier for debugging
#endif
        
        [SerializeField] private TrafficCar[] trafficCarPrefabs;

        public TrafficCar SpawnCar(Chunk chunk, Vector3 position, Quaternion rotation)
        {
            TrafficCar randomCarVariant = trafficCarPrefabs.GetRandom().gameObject.GetSpareOrCreate<TrafficCar>(transform, position, rotation);
            randomCarVariant.Initialise(chunk);
            
#if UNITY_EDITOR
            randomCarVariant.name = $"TrafficCar-{randomCarVariant.gameObject.name}-{trafficCarID}";
            trafficCarID++;
#endif
            
            return randomCarVariant;
        }

    }
}
