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
            if (currentTyre.OutsideVertices.Length == 0 || currentTyre.InsideVertices.Length == 0)
            {
                Debug.LogError("Cannot stretch tyre - tyre data hasn't been calculated.");
                return;
            }
            
            if (currentRim.OutsideVertices.Length == 0 || currentRim.InsideVertices.Length == 0)
            {
                Debug.LogError("Cannot stretch tyre - rim data hasn't been calculated.");
                return;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return; //dont stretch outside playmode to avoid editing the mesh 
#endif
            
            //if modifying the tyre mesh, need to make sure the tyre is a copy
            if (!isTyreCopied)
                CopyTyre();

            //do both directions
            MoveVertices(true);
            MoveVertices(false);
        }
        
        private void CopyTyre()
        {
            if (!Application.isPlaying)
                return;
            
            isTyreCopied = true;
            
            currentTyre.MeshFilter.sharedMesh = Instantiate(currentTyre.MeshFilter.sharedMesh);
        }
        
        private void MoveVertices(bool inside)
        {
            int[] tyreEndVertices = inside ? currentTyre.InsideVertices : currentTyre.OutsideVertices;
            
            Vector3[] verticesCopy = currentTyre.MeshFilter.sharedMesh.vertices;
            
            //move the tyre end vertices to the rim end vertices
            foreach (int vertexIndex in tyreEndVertices)
            {
                Vector3 vertexPositionLocal = currentTyre.MeshFilter.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = currentTyre.MeshFilter.transform.TransformPoint(vertexPositionLocal);

                //cache the closest vertex position on rim if not already cached
                if (!closestVertexIndiciesOnBarrelCached.ContainsKey(vertexIndex))
                    closestVertexIndiciesOnBarrelCached[vertexIndex] = GetClosestVertexIndexOnBarrel(inside, vertexPositionWorld);

                Vector3 closestRimPositionLocal = currentRim.Barrel.sharedMesh.vertices[closestVertexIndiciesOnBarrelCached[vertexIndex]];
                Vector3 closestRimPositionWorld = currentRim.Barrel.transform.TransformPoint(closestRimPositionLocal);
                
                Vector3 closestRimTyrePositionLocal = currentTyre.MeshFilter.transform.InverseTransformPoint(closestRimPositionWorld);

                verticesCopy[vertexIndex] = closestRimTyrePositionLocal;
                
                Debug.DrawLine(vertexPositionWorld, closestRimPositionWorld, Color.red, 15);
            }
            
            currentTyre.MeshFilter.sharedMesh.SetVertices(verticesCopy);
            currentTyre.MeshFilter.sharedMesh.RecalculateBounds();
        }

        private int GetClosestVertexIndexOnBarrel(bool inside, Vector3 worldPosition)
        {
            int[] rimEndVertices = inside ? currentRim.InsideVertices : currentRim.OutsideVertices;
            
            float closestDistanceSqr = Mathf.Infinity;
            int closestIndex = -1;
            
            foreach (int vertexIndex in rimEndVertices)
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
