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

        public Rim Rim => currentRim;
        public Tyre Tyre => currentTyre;

        private bool isTyreCopied;
        private readonly Dictionary<int, int> closestVertexIndiciesOnBarrelCached = new();

        /// <summary>
        /// Ability to change the rim at runtime.
        /// </summary>
        public void SetRim(Rim rim)
        {
            currentRim = rim;

            //reset the cache as the rim has changed
            closestVertexIndiciesOnBarrelCached.Clear();
        }
        
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
            
#if UNITY_EDITOR
            //copy the mesh if editing in editor - to prevent editing the main mesh
            if (!Application.isPlaying)
            {
                Debug.LogError("Cannot stretch tyre while application is not running.");
                return;
            }
#endif
            
            //if modifying the tyre mesh, need to make sure the tyre is a copy
            if (!isTyreCopied)
                CopyTyre();

            MoveVertices();
        }
        
        private void CopyTyre()
        {
            if (!Application.isPlaying)
                return;
            
            isTyreCopied = true;
            
            currentTyre.MeshFilter.sharedMesh = Instantiate(currentTyre.MeshFilter.sharedMesh);
        }
        
        private void MoveVertices()
        {
            Vector3[] verticesCopy = currentTyre.MeshFilter.sharedMesh.vertices;
            
            //move the tyre end vertices to the rim end vertices
            foreach (int vertexIndex in currentTyre.VerticesClosestToMiddle)
            {
                Vector3 vertexPositionLocal = currentTyre.MeshFilter.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = currentTyre.MeshFilter.transform.TransformPoint(vertexPositionLocal);

                //cache the closest vertex position on rim if not already cached
                if (!closestVertexIndiciesOnBarrelCached.ContainsKey(vertexIndex))
                    closestVertexIndiciesOnBarrelCached[vertexIndex] = GetClosestVertexIndexOnBarrel(vertexPositionWorld);

                Vector3 closestRimPositionLocal = currentRim.Barrel.sharedMesh.vertices[closestVertexIndiciesOnBarrelCached[vertexIndex]];
                Vector3 closestRimPositionWorld = currentRim.Barrel.transform.TransformPoint(closestRimPositionLocal);
                
                Vector3 closestRimTyrePositionLocal = currentTyre.MeshFilter.transform.InverseTransformPoint(closestRimPositionWorld);

                verticesCopy[vertexIndex] = closestRimTyrePositionLocal;
                
                Debug.DrawLine(vertexPositionWorld, closestRimPositionWorld, Color.red, 15);
            }
            
            currentTyre.MeshFilter.sharedMesh.SetVertices(verticesCopy);
        }

        private int GetClosestVertexIndexOnBarrel(Vector3 worldPosition)
        {
            float closestDistanceSqr = Mathf.Infinity;
            int closestIndex = -1;
            
            foreach (int vertexIndex in currentRim.VerticesFurthestFromMiddle)
            {
                Vector3 vertexPositionLocal = currentRim.Barrel.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = currentRim.Barrel.transform.TransformPoint(vertexPositionLocal);

                float distanceSqr = Vector3.SqrMagnitude(vertexPositionWorld - worldPosition);
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestIndex = vertexIndex;
                }
            }

            return closestIndex;
        }

    }
}
