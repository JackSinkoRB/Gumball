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
    [RequireComponent(typeof(MeshFilter))]
    public class Tyre : MonoBehaviour
    {

        [Header("Debugging")]
        [SerializeField, ReadOnly] private int[] outsideVertices;
        [SerializeField, ReadOnly] private int[] insideVertices;

        public MeshFilter MeshFilter => GetComponent<MeshFilter>();

        public int[] OutsideVertices => outsideVertices;
        public int[] InsideVertices => insideVertices;

#if UNITY_EDITOR
        
        [ButtonMethod]
        public void CalculateData()
        {
            //do both directions
            FindVerticesClosestToMiddle(true);
            FindVerticesClosestToMiddle(false);
            
            EditorUtility.SetDirty(this);
        }

        private void FindVerticesClosestToMiddle(bool inside)
        {
            Vector3 direction = inside ? MeshFilter.transform.forward : -MeshFilter.transform.forward;
            
            Debug.DrawLine(MeshFilter.transform.position, MeshFilter.transform.position + direction, Color.red, 15);
            HashSet<int> verticesClosestToMiddleTemp = new();
            Vector3 middle = MeshFilter.transform.position + direction;

            //1. find the closest distance
            float closestDistanceSqr = Mathf.Infinity;
            for (int vertexIndex = 0; vertexIndex < MeshFilter.sharedMesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPositionLocal = MeshFilter.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = MeshFilter.transform.TransformPoint(vertexPositionLocal);

                float distanceToMiddleSqr = Vector3.SqrMagnitude(middle - vertexPositionWorld);

                if (distanceToMiddleSqr < closestDistanceSqr)
                    closestDistanceSqr = distanceToMiddleSqr;
            }
            
            //2. because tyre is circular, all the vertices will have same distance, so check for the closest distance
            for (int vertexIndex = 0; vertexIndex < MeshFilter.sharedMesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPositionLocal = MeshFilter.sharedMesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = MeshFilter.transform.TransformPoint(vertexPositionLocal);

                float distanceToMiddleSqr = Vector3.SqrMagnitude(middle - vertexPositionWorld);

                if (distanceToMiddleSqr.Approximately(closestDistanceSqr, 0.001f))
                {
                    verticesClosestToMiddleTemp.Add(vertexIndex);
                    Debug.DrawLine(vertexPositionWorld, middle, Color.blue, 15);
                }
            }

            if (inside)
                insideVertices = verticesClosestToMiddleTemp.ToArray();
            else
                outsideVertices = verticesClosestToMiddleTemp.ToArray();
        }
        
#endif
        
    }
}
