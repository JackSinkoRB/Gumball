using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshHelper : MonoBehaviour
    {

        private MeshFilter meshFilter => GetComponent<MeshFilter>();

        public Vector3 GetVertexWorldPosition(int vertexIndex)
        {
            var vertexLocalPosition = meshFilter.sharedMesh.vertices[vertexIndex];
            Vector3 vertexWorldPosition = meshFilter.transform.TransformPoint(vertexLocalPosition);
            return vertexWorldPosition;
        }
        
        private void DrawNormal(int vertexIndex, Color color)
        {
            const float distance = 5;
            const float duration = 120;

            Debug.DrawRay(GetVertexWorldPosition(vertexIndex), meshFilter.sharedMesh.normals[vertexIndex] * distance, color, duration);
        }

        [ButtonMethod]
        public void DrawAllNormals()
        {
            for (int vertexIndex = 0; vertexIndex < meshFilter.sharedMesh.vertices.Length; vertexIndex++)
            {
                DrawNormal(vertexIndex, Color.red);
            }
        }
        
    }
}
