using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class TrafficCarSpawner : Singleton<TrafficCarSpawner>
    {

        [SerializeField] private TrafficCar[] trafficCarPrefabs;

        public TrafficCar SpawnCar(Vector3 position)
        {
            TrafficCar randomCarVariant = trafficCarPrefabs.GetRandom().gameObject.GetSpareOrCreate<TrafficCar>(position: position);
            return randomCarVariant;
        }
        
    }
}
