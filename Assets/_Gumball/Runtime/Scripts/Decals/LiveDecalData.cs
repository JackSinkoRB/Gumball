using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
        [Serializable]
        public struct LiveDecalData
        {
            [SerializeField] private int categoryIndex;
            [SerializeField] private int textureIndex;
            [SerializeField] private int priority;
            [SerializeField] private SerializedVector3 localPositionToCar;
            [SerializeField] private SerializedVector3 localRotationToCar;
            [SerializeField] private SerializedVector3 lastKnownHitNormal;
            [SerializeField] private SerializedVector3 scale;
            [SerializeField] private float angle;
            [SerializeField] private int colorIndex;

            public int CategoryIndex => categoryIndex;
            public int TextureIndex => textureIndex;
            public int Priority => priority;
            public SerializedVector3 LocalPositionToCar => localPositionToCar;
            public SerializedVector3 LocalRotationToCar => localRotationToCar;
            public SerializedVector3 LastKnownHitNormal => lastKnownHitNormal;
            
            public SerializedVector3 Scale => scale;
            public float Angle => angle;
            public int ColorIndex => colorIndex;
            
            public LiveDecalData(LiveDecal liveDecal)
            {
                categoryIndex = liveDecal.CategoryIndex;
                textureIndex = liveDecal.TextureIndex;
                priority = liveDecal.Priority;
                
                //save relative to car:
                localPositionToCar = liveDecal.LastKnownLocalPosition.ToSerializedVector();
                localRotationToCar = liveDecal.LastKnownRotation.eulerAngles.ToSerializedVector();
                lastKnownHitNormal = liveDecal.LastKnownHitNormal.ToSerializedVector();
                
                scale = liveDecal.Scale.ToSerializedVector();
                angle = liveDecal.Angle;
                colorIndex = liveDecal.ColorIndex;
            }

            public LiveDecalData(int categoryIndex, int textureIndex, int priority, Vector3 positionOffsetFromCar, Vector3 rotationOffsetFromCar, Vector3 hitNormal, Vector3 scale, float angle, int colorIndex)
            {
                this.categoryIndex = categoryIndex;
                this.textureIndex = textureIndex;
                this.priority = priority;
                this.localPositionToCar = positionOffsetFromCar.ToSerializedVector();
                this.localRotationToCar = rotationOffsetFromCar.ToSerializedVector();
                this.lastKnownHitNormal = hitNormal.ToSerializedVector();
                this.scale = scale.ToSerializedVector();
                this.angle = angle;
                this.colorIndex = colorIndex;
            }
            
        }
}
