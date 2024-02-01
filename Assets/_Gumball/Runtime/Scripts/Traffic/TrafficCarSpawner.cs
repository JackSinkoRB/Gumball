using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class TrafficCarSpawner : Singleton<TrafficCarSpawner>
    {

        [SerializeField] private TrafficCar[] trafficCarPrefabs;

        public TrafficCar SpawnCar(Chunk chunk, Vector3 position, Quaternion rotation)
        {
            TrafficCar randomCarVariant = trafficCarPrefabs.GetRandom().gameObject.GetSpareOrCreate<TrafficCar>(transform, position, rotation);
            randomCarVariant.Initialise(chunk);
            return randomCarVariant;
        }

    }
}
