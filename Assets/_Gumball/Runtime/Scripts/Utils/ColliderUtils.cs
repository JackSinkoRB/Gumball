using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class ColliderUtils
    {
        
        /// <returns>The closest vertex position, and the distance (squared) to the vertex from the given position.</returns>
        public static (Vector3, float) ClosestVertex(this Collider collider, Vector3 position, bool flatten = false)
        {
            if (collider is not MeshCollider meshCollider || meshCollider.convex)
            {
                Vector3 closestPoint = collider.ClosestPoint(position);
                return (closestPoint, (closestPoint - position).sqrMagnitude);
            }

            if (meshCollider.sharedMesh == null || meshCollider.sharedMesh.vertices.Length == 0)
                throw new InvalidOperationException($"The mesh for {collider.gameObject.name}'s MeshCollider is invalid.");

            if (flatten)
                position = position.Flatten();
            
            Vector3 closestVertex = default;
            float closestDistanceSqr = -1;
            foreach (Vector3 vertexPosition in meshCollider.sharedMesh.vertices)
            {
                Vector3 vertexPositionWorld = meshCollider.transform.TransformPoint(vertexPosition);

                //check to flatten
                Vector3 vertexPositionFlattened = vertexPositionWorld.Flatten();
                if (flatten)
                    vertexPositionFlattened = vertexPositionWorld.Flatten();

                float distanceSqr = (vertexPositionFlattened - position).sqrMagnitude;
                if (closestDistanceSqr < 0 || distanceSqr < closestDistanceSqr)
                {
                    closestVertex = vertexPositionWorld;
                    closestDistanceSqr = distanceSqr;
                }
            }

            return (closestVertex, closestDistanceSqr);
        }
        
    }
}
