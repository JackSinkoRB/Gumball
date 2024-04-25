using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class WheelMesh : MonoBehaviour
    {
        
        [SerializeField] private Rim currentRim; //TODO: set dynamically when rim is spawned
        [SerializeField] private Tyre currentTyre;

        [ButtonMethod]
        public void StretchTyre()
        {
            if (currentTyre.VerticesClosestToMiddle.Length == 0)
            {
                Debug.LogError("Cannot stretch tyre - tyre data hasn't been calculated.");
                return;
            }
            
            if (currentRim.VerticesFurthestFromMiddle.Length == 0)
            {
                Debug.LogError("Cannot stretch tyre - rim data hasn't been calculated.");
                return;
            }
            
            //copy the mesh if editing in editor - to prevent editing the main mesh
            if (!Application.isPlaying)
                currentTyre.MeshFilter.sharedMesh = Instantiate(currentTyre.MeshFilter.sharedMesh);

            MoveVertices();
        }

        private void MoveVertices()
        {
            Vector3[] verticesCopy = currentTyre.MeshFilter.sharedMesh.vertices;
            
            //move the tyre end vertices to the rim end vertices
            foreach (int vertexIndex in currentTyre.VerticesClosestToMiddle)
            {
                Vector3 vertexPositionLocal = currentTyre.MeshFilter.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = currentTyre.MeshFilter.transform.TransformPoint(vertexPositionLocal);

                Vector3 closestRimPositionWorld = GetClosestVertexPositionOnRim(vertexPositionWorld);
                Vector3 closestRimPositionLocal = currentTyre.MeshFilter.transform.InverseTransformPoint(closestRimPositionWorld);
                
                verticesCopy[vertexIndex] = closestRimPositionLocal;
                
                Debug.DrawLine(vertexPositionWorld, closestRimPositionWorld, Color.red, 15);
            }
            
            currentTyre.MeshFilter.sharedMesh.SetVertices(verticesCopy);
        }

        private Vector3 GetClosestVertexPositionOnRim(Vector3 worldPosition)
        {
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 closestPosition = Vector3.zero;
            
            foreach (int vertexIndex in currentRim.VerticesFurthestFromMiddle)
            {
                Vector3 vertexPositionLocal = currentRim.Barrel.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = currentRim.Barrel.transform.TransformPoint(vertexPositionLocal);

                float distanceSqr = Vector3.SqrMagnitude(vertexPositionWorld - worldPosition);
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestPosition = vertexPositionWorld;
                }
            }

            return closestPosition;
        }

    }
}
