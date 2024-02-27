using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct PositionAndRotation
    {

        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;

        public Vector3 Position => position;
        public Quaternion Rotation => rotation;
        
        public PositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
        
    }
}
