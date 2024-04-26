using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    public class Rim : MonoBehaviour
    {
        
        [SerializeField] private MeshFilter barrel;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private int[] outsideVertices;
        [SerializeField, ReadOnly] private int[] insideVertices;

        public MeshFilter Barrel => barrel;
        public int[] OutsideVertices => outsideVertices;
        public int[] InsideVertices => insideVertices;

#if UNITY_EDITOR
        
        [ButtonMethod]
        public void CalculateData()
        {
            //do both directions
            FindVerticesFurthestFromMiddle(true);
            FindVerticesFurthestFromMiddle(false);
            
            EditorUtility.SetDirty(this);
        }

        private void FindVerticesFurthestFromMiddle(bool inside)
        {
            MeshFilter meshFilter = barrel.GetComponent<MeshFilter>();
            Vector3 direction = inside ? -meshFilter.transform.right : meshFilter.transform.right;

            Debug.DrawLine(meshFilter.transform.position, meshFilter.transform.position + direction, Color.red, 15);
            HashSet<int> verticesFurthestFromMiddleTemp = new();
            Vector3 middle = meshFilter.transform.position + direction;

            //1. find the furthest distance
            float furthestDistanceSqr = 0;
            for (int vertexIndex = 0; vertexIndex < meshFilter.sharedMesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPositionLocal = meshFilter.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = meshFilter.transform.TransformPoint(vertexPositionLocal);

                float distanceToMiddleSqr = Vector3.SqrMagnitude(middle - vertexPositionWorld);

                if (distanceToMiddleSqr > furthestDistanceSqr)
                    furthestDistanceSqr = distanceToMiddleSqr;
            }
            
            //2. because tyre is circular, all the vertices will have same distance, so check for the closest distance
            for (int vertexIndex = 0; vertexIndex < meshFilter.sharedMesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPositionLocal = meshFilter.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = meshFilter.transform.TransformPoint(vertexPositionLocal);

                float distanceToMiddleSqr = Vector3.SqrMagnitude(middle - vertexPositionWorld);

                if (distanceToMiddleSqr.Approximately(furthestDistanceSqr, 0.001f))
                {
                    verticesFurthestFromMiddleTemp.Add(vertexIndex);
                    Debug.DrawLine(vertexPositionWorld, middle, Color.blue, 15);
                }
            }

            if (inside)
                insideVertices = verticesFurthestFromMiddleTemp.ToArray();
            else 
                outsideVertices = verticesFurthestFromMiddleTemp.ToArray();
        }
        
#endif
        
    }
}
