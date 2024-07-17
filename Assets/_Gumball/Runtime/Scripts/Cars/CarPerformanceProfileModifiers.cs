using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct CarPerformanceProfileModifiers
    {

        [SerializeField, Range(0, 1)] private float maxSpeed;
        [SerializeField, Range(0, 1)] private float acceleration;
        [SerializeField, Range(0, 1)] private float handling;
        [SerializeField, Range(0, 1)] private float nos;

        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;
        public float Handling => handling;
        public float Nos => nos;

    }
}
