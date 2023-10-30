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
            [SerializeField, ReadOnly] private Chunk chunkBelongsTo;
            
            public int Index => index;
            public Vector3 LocalPosition => localPosition;
            public Vector3 WorldPosition => MeshBelongsTo.MeshFilter.transform.TransformPoint(LocalPosition);
            public ChunkMeshData MeshBelongsTo => chunkBelongsTo.ChunkMeshData;

            public Vertex(int index, Vector3 localPosition, Chunk chunkBelongsTo)
            {
                this.index = index;
                this.localPosition = localPosition;
                this.chunkBelongsTo = chunkBelongsTo;
            }

            /// <summary>
            /// Because the mesh can be updated but not yet applied, use this to get the current (but not applied) value instead.
            /// </summary>
            public Vector3 GetCurrentWorldPosition()
            {
                Vector3 previousPositionLocal = MeshBelongsTo.vertices[Index];
                Vector3 previousPositionWorld = MeshBelongsTo.MeshFilter.transform.TransformPoint(previousPositionLocal);
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
        [SerializeField, ReadOnly] private Vector3[] vertices;
        [SerializeField, ReadOnly] private List<Vertex> lastEndVertices;
        [SerializeField, ReadOnly] private List<Vertex> firstEndVertices;

        public Chunk Chunk => chunk;
        public MeshFilter MeshFilter => chunk.CurrentTerrain.GetComponent<MeshFilter>();
        public Mesh Mesh => MeshFilter.sharedMesh;
        public Vector3[] Vertices => vertices;
        public ReadOnlyCollection<Vertex> LastEndVertices => lastEndVertices.AsReadOnly();
        public ReadOnlyCollection<Vertex> FirstEndVertices => firstEndVertices.AsReadOnly();
        
        public ChunkMeshData(Chunk chunk)
        {
            this.chunk = chunk;

            vertices = Mesh.vertices;

            FindVerticesOnTangents();
        }

        /// <summary>
        /// Because the mesh can be updated but not yet applied, use this to get the current (but not applied) value instead.
        /// </summary>
        public Vector3 GetCurrentVertexWorldPosition(int index)
        {
            Vector3 previousPositionLocal = vertices[index];
            Vector3 previousPositionWorld = MeshFilter.transform.TransformPoint(previousPositionLocal);
            return previousPositionWorld;
        }

        public void SetVertexWorldPosition(int index, Vector3 worldPosition)
        {
            vertices[index] = MeshFilter.transform.InverseTransformPoint(worldPosition);
        }

        public void SetVertices(Vector3[] vertices)
        {
            this.vertices = vertices;
        }
        
        public void ApplyChanges()
        {
            Mesh meshToUse = Object.Instantiate(Mesh); //use a mesh copy so that we're not editing the actual shared mesh, and so that it can be undone in editor

            meshToUse.SetVertices(vertices);

            //recalculate UVs
            meshToUse.SetUVs(0, ChunkUtils.GetTriplanarUVs(vertices, MeshFilter.transform));

            meshToUse.RecalculateTangents();
            meshToUse.RecalculateNormals();
            //meshToUse.SetNormals(CalculateNormals());
            meshToUse.RecalculateBounds();
            
            MeshFilter.sharedMesh = meshToUse; //set the mesh copy so that we're not editing the actual shared mesh, and so that it can be undone in editor
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
            
            chunk.UpdateSplineImmediately(); //this is required as the chunk is rotated
                
            //get the vertices on each chunks tangent
            Vector3 lastPoint = chunk.LastSample.position;
            lastEndVertices = GetVerticesOnTangent(lastPoint - chunk.LastTangent, lastPoint + chunk.LastTangent);
            Debug.DrawLine(lastPoint - chunk.LastTangent * 200, lastPoint + chunk.LastTangent * 200, Color.magenta, 15);
            GlobalLoggers.ChunkLogger.Log($"Found {lastEndVertices.Count} vertices at the end of ({chunk.gameObject.name}) - position = {lastPoint}.");

            Vector3 firstPoint = chunk.FirstSample.position;
            firstEndVertices = GetVerticesOnTangent(firstPoint - chunk.FirstTangent, firstPoint + chunk.FirstTangent);
            Debug.DrawLine(firstPoint - chunk.FirstTangent * 200, firstPoint + chunk.FirstTangent * 200, Color.magenta, 15);
            GlobalLoggers.ChunkLogger.Log($"Found {firstEndVertices.Count} vertices at the end of ({chunk.gameObject.name}) - position = {lastPoint}.");

            chunk.transform.rotation = previousRotation;
        }
        
        private List<Vertex> GetVerticesOnTangent(Vector3 tangentStart, Vector3 tangentEnd)
        {
            List<Vertex> verticesOnTangent = new();

            for (var vertexIndex = 0; vertexIndex < Mesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = Mesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = MeshFilter.transform.TransformPoint(vertexPosition);

                Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 10, Color.yellow, 15);

                if (IsPointOnTangent(vertexPositionWorld, tangentStart, tangentEnd))
                {
                    verticesOnTangent.Add(new Vertex(vertexIndex, vertexPosition, chunk));
                    Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 20, Color.magenta, 15);
                }
            }

            return verticesOnTangent;
        }

        private bool IsPointOnTangent(Vector3 point, Vector3 tangentStart, Vector3 tangentEnd)
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