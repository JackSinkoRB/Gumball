using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyBox;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gumball
{
    [Serializable]
    public class ChunkMeshData
    {
        [Serializable]
        public struct Vertex : IEquatable<Vertex>
        {
            [SerializeField, ReadOnly] private int index;
            [SerializeField, ReadOnly] private Vector3 localPosition;
            private ChunkMeshData meshBelongsTo;
            
            public int Index => index;
            public Vector3 LocalPosition => localPosition;
            public Vector3 WorldPosition => MeshBelongsTo.meshFilter.transform.TransformPoint(LocalPosition);
            public ChunkMeshData MeshBelongsTo => meshBelongsTo;

            public Vertex(int index, Vector3 localPosition, ChunkMeshData meshBelongsTo)
            {
                this.index = index;
                this.localPosition = localPosition;
                this.meshBelongsTo = meshBelongsTo;
            }

            /// <summary>
            /// Because the mesh can be updated but not yet applied, use this to get the current (but not applied) value instead.
            /// </summary>
            public Vector3 GetCurrentWorldPosition()
            {
                Vector3 previousPositionLocal = MeshBelongsTo.vertices[Index];
                Vector3 previousPositionWorld = MeshBelongsTo.meshFilter.transform.TransformPoint(previousPositionLocal);
                return previousPositionWorld;
            }

            public bool Equals(Vertex other)
            {
                return Index == other.Index && Equals(MeshBelongsTo, other.MeshBelongsTo);
            }

            public override bool Equals(object obj)
            {
                return obj is Vertex other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Index, MeshBelongsTo);
            }
        }
        
        [SerializeField, ReadOnly] private Chunk chunk;
        [SerializeField, ReadOnly] private MeshFilter meshFilter;
        [SerializeField, ReadOnly] private Mesh mesh;
        [SerializeField, ReadOnly] private Vector3[] vertices;
        [SerializeField, ReadOnly] private List<Vertex> lastEndVertices;
        [SerializeField, ReadOnly] private List<Vertex> firstEndVertices;

        public Chunk Chunk => chunk;
        public MeshFilter MeshFilter => meshFilter;
        public Mesh Mesh => mesh;
        public Vector3[] Vertices => vertices;
        public ReadOnlyCollection<Vertex> LastEndVertices => lastEndVertices.AsReadOnly();
        public ReadOnlyCollection<Vertex> FirstEndVertices => firstEndVertices.AsReadOnly();
        
        internal ChunkMeshData(Chunk chunk)
        {
            this.chunk = chunk;

            meshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
            mesh = meshFilter.sharedMesh;
            vertices = mesh.vertices;

            FindVerticesOnTangents();
        }

        internal void SetVertexWorldPosition(int index, Vector3 worldPosition)
        {
            vertices[index] = meshFilter.transform.InverseTransformPoint(worldPosition);
        }

        /// <summary>
        /// Because the mesh can be updated but not yet applied, use this to get the current (but not applied) value instead.
        /// </summary>
        public Vector3 GetCurrentVertexWorldPosition(int index)
        {
            Vector3 previousPositionLocal = vertices[index];
            Vector3 previousPositionWorld = meshFilter.transform.TransformPoint(previousPositionLocal);
            return previousPositionWorld;
        }

        internal void ApplyChanges()
        {
            Mesh meshToUse = mesh;
#if UNITY_EDITOR
            meshToUse = Object
                .Instantiate(
                    mesh); //use a mesh copy so that we're not editing the actual shared mesh, and so that it can be undone in editor
#endif

            meshToUse.SetVertices(vertices);

            //recalculate UVs
            meshToUse.SetUVs(0, ChunkUtils.GetTriplanarUVs(vertices, meshFilter.transform));

            meshToUse.RecalculateTangents();
            meshToUse.RecalculateNormals();
            //meshToUse.SetNormals(CalculateNormals());
            meshToUse.RecalculateBounds();

#if UNITY_EDITOR
            meshFilter.sharedMesh =
                meshToUse; //set the mesh copy so that we're not editing the actual shared mesh, and so that it can be undone in editor
#endif
        }

        private Vector3[] CalculateNormals()
        {
            //TODO:
            //1. recalculate the normals like normal, but if the vertex is an end vertex, get the opposite end vertex
            //2. could calculate normals based on the vertex height
            //3. allow for 1 extra row of vertices after the tangent, but don't let them be seen

            Vector3[] vertexNormals = new Vector3[vertices.Length];
            return vertexNormals;
        }
        
        private void FindVerticesOnTangents()
        {
            Quaternion previousRotation = chunk.transform.rotation;
            chunk.transform.rotation = Quaternion.Euler(new Vector3(0, chunk.transform.rotation.eulerAngles.y, 0));
            
            ChunkUtils.UpdateSplineImmediately(chunk); //this is required as the chunk is rotated
                
            //get the vertices on each chunks tangent
            Vector3 lastPoint = chunk.LastSample.position;
            lastEndVertices = GetVerticesOnTangent(this, lastPoint - chunk.LastTangent, lastPoint + chunk.LastTangent);
            Debug.DrawLine(lastPoint - chunk.LastTangent * 200, lastPoint + chunk.LastTangent * 200, Color.magenta, 15);
            GlobalLoggers.TerrainLogger.Log($"Found {lastEndVertices.Count} vertices at the end of ({chunk.gameObject.name}) - position = {lastPoint}.");

            Vector3 firstPoint = chunk.FirstSample.position;
            firstEndVertices = GetVerticesOnTangent(this, firstPoint - chunk.FirstTangent, firstPoint + chunk.FirstTangent);
            Debug.DrawLine(firstPoint - chunk.FirstTangent * 200, firstPoint + chunk.FirstTangent * 200, Color.magenta, 15);
            GlobalLoggers.TerrainLogger.Log($"Found {firstEndVertices.Count} vertices at the end of ({chunk.gameObject.name}) - position = {lastPoint}.");

            chunk.transform.rotation = previousRotation;
        }
        
        private static List<Vertex> GetVerticesOnTangent(ChunkMeshData chunkMeshData, Vector3 tangentStart, Vector3 tangentEnd)
        {
            List<Vertex> verticesOnTangent = new();

            for (var vertexIndex = 0; vertexIndex < chunkMeshData.mesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = chunkMeshData.mesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = chunkMeshData.meshFilter.transform.TransformPoint(vertexPosition);

                Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 10, Color.yellow, 15);

                if (IsPointOnTangent(vertexPositionWorld, tangentStart, tangentEnd))
                {
                    verticesOnTangent.Add(new Vertex(vertexIndex, vertexPosition, chunkMeshData));
                    Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 20, Color.magenta, 15);
                }
            }

            return verticesOnTangent;
        }

        private static bool IsPointOnTangent(Vector3 point, Vector3 tangentStart, Vector3 tangentEnd)
        {
            const float tolerance = 0.5f;
            
            Vector2 tangentDirection = (tangentEnd.FlattenAsVector2() - tangentStart.FlattenAsVector2()).normalized;
            Vector2 perpendicularDirection = new Vector2(-tangentDirection.y, tangentDirection.x);
            Vector2 vectorToTarget = point.FlattenAsVector2() - tangentStart.FlattenAsVector2();

            float dotProduct = Vector2.Dot(vectorToTarget, perpendicularDirection);

            return Mathf.Abs(dotProduct) < tolerance;
        }
    }
}