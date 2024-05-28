using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public readonly struct TransformOperation
    {
            
        public Transform Target { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
            
        public TransformOperation(Transform target, Vector3 position, Quaternion rotation)
        {
            Target = target;
            Position = position;
            Rotation = rotation;
        }

        public void Apply()
        {
            Target.position = Position;
            Target.rotation = Rotation;
        }

        /// <summary>
        /// Blend the current operation with the desired operation, with a weighting (between 0 and 1).
        /// </summary>
        public bool TryBlend(TransformOperation desired, float blendWeight = 0.5f)
        {
            if (Target != desired.Target)
                return false; //if it doesn't match, just return the current
            
            Vector3 positionBlended = Vector3.Lerp(Position, desired.Position, blendWeight);
            Quaternion rotationBlended = Quaternion.Slerp(Rotation, desired.Rotation, blendWeight);

            Debug.DrawLine(Position, positionBlended, Color.red);
            Debug.DrawLine(desired.Position, positionBlended, Color.blue);
            
            TransformOperation blendedOperation = new TransformOperation(Target, positionBlended, rotationBlended);
            blendedOperation.Apply();
            
            return true;
        }
        
    }
}
