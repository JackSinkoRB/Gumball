using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JBooth.VertexPainterPro;
using MyBox;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gumball
{
    [Serializable]
    public class ChunkMeshData
    {
        
        [SerializeField, ReadOnly] private Chunk chunk;
        [SerializeField, ReadOnly] private Vector3[] vertices;
        [SerializeField, ReadOnly] private Vector3[] normals;
        [SerializeField, ReadOnly] private GenericDictionary<int, Vector3> modifiedNormals = new();
        [SerializeField, ReadOnly] private List<int> lastEndVertices;
        [SerializeField, ReadOnly] private List<int> firstEndVertices;
        [SerializeField, ReadOnly] private List<int> verticesExcludingEnds;
        [SerializeField, ReadOnly] private SerializableColor[] vertexColors;
        
        public Chunk Chunk => chunk;
        public MeshFilter MeshFilter => chunk.TerrainHighLOD.GetComponent<MeshFilter>();
        public MeshCollider MeshCollider => chunk.TerrainHighLOD.GetComponent<MeshCollider>();
        public Mesh Mesh => MeshFilter.sharedMesh;
        public Vector3[] Vertices => vertices;
        public Vector3[] Normals => normals;
        public SerializableColor[] VertexColors => vertexColors;
        public ReadOnlyCollection<int> LastEndVertices => lastEndVertices.AsReadOnly();
        public ReadOnlyCollection<int> FirstEndVertices => firstEndVertices.AsReadOnly();
        public ReadOnlyCollection<int> VerticesExcludingEnds => verticesExcludingEnds.AsReadOnly();

        public ChunkMeshData(Chunk chunk)
        {
            this.chunk = chunk;

            vertices = Mesh.vertices;

            FindVerticesOnTangents();
        }

        public ChunkMeshData()
        {
            
        }
        
        public ChunkMeshData Clone()
        {
            ChunkMeshData copy = new ChunkMeshData();
            copy.chunk = chunk;
            copy.vertices = vertices;
            copy.normals = normals;
            copy.modifiedNormals = modifiedNormals;
            copy.lastEndVertices = lastEndVertices;
            copy.firstEndVertices = firstEndVertices;
            copy.verticesExcludingEnds = verticesExcludingEnds;
            copy.vertexColors = vertexColors;
#if UNITY_EDITOR
            copy.additionalVertexPaintData = additionalVertexPaintData;
#endif

            MeshFilter.sharedMesh = Object.Instantiate(Mesh); //copy the mesh so not directly editing

            return copy;
        }

        public void SetChunk(Chunk chunk)
        {
            this.chunk = chunk;
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
        
#if UNITY_EDITOR
        public void SetNormals(Vector3[] normals)
        {
            //apply modified normals
            foreach (int vertexIndex in modifiedNormals.Keys)
            {
                Vector3 modifiedNormal = modifiedNormals[vertexIndex];

                normals[vertexIndex] = modifiedNormal;
            }
            
            this.normals = normals;

            if (MeshFilter.sharedMesh != null)
                MeshFilter.sharedMesh.SetNormals(normals);
        }

        public void UpdateNormals()
        {
            SetNormals(normals);
        }

        public void ModifyNormal(int vertexIndex, Vector3 normal)
        {
            modifiedNormals[vertexIndex] = normal;
        }
#endif

        public void ApplyChanges()
        {
            if (Mesh == null)
                throw new NullReferenceException($"Terrain is missing its mesh on {chunk.gameObject.name}. The baked asset may have been lost. Try recreating the terrain.");

            Mesh meshToUse = Object.Instantiate(Mesh); //use a mesh copy so that we're not editing the actual shared mesh, and so that it can be undone in editor

            meshToUse.SetVertices(vertices);
            meshToUse.SetNormals(normals);
            meshToUse.SetColors(vertexColors.ToColors());

            //recalculate UVs
            meshToUse.SetUVs(0, ChunkUtils.GetTriplanarUVs(vertices, MeshFilter.transform));

            meshToUse.RecalculateTangents();
            meshToUse.RecalculateBounds();

            MeshFilter.sharedMesh = meshToUse;
            
            if (meshToUse.vertices.Length < 3)
                Debug.LogError($"The terrain mesh for {chunk.name} has too little vertices for a collider. Does it need to be rebaked?");
            else MeshCollider.sharedMesh = meshToUse;
        }

        public void UpdateVertexColors(SerializableColor[] colors)
        {
            vertexColors = colors;
            MeshFilter.sharedMesh.colors = vertexColors.ToColors();
        }
        
#if UNITY_EDITOR
        private GenericDictionary<int, List<VertexInstanceStream.PaintData>> additionalVertexPaintData = new();
        
        public void TrackPaintData(int index, VertexInstanceStream.PaintData data)
        {
            if (!additionalVertexPaintData.ContainsKey(index))
                additionalVertexPaintData[index] = new List<VertexInstanceStream.PaintData>();
         
            additionalVertexPaintData[index].Add(data);
        }
        
        public Color[] CalculateVertexColors()
        {
            TerrainTextureBlendSettings terrainBlendSettings = chunk.GetComponent<ChunkEditorTools>().TerrainData.TextureBlendSettings;
            Color[] colors = terrainBlendSettings.GetVertexColors(chunk, vertices, MeshFilter.transform, Mesh);

            ApplyPaintData(colors); //re-add the paint data
            UpdateVertexColors(colors.ToSerializableColors());
            
            return colors;
        }

        private void ApplyPaintData(Color[] colors)
        {
            foreach (int index in additionalVertexPaintData.Keys)
            {
                List<VertexInstanceStream.PaintData> dataCollection = additionalVertexPaintData[index];
                
                foreach (VertexInstanceStream.PaintData data in dataCollection)
                {
                    colors[index] = Color.Lerp(colors[index], data.color, data.strength);
                }
            }
        }
#endif

        private void FindVerticesOnTangents()
        {
            Quaternion previousRotation = chunk.transform.rotation;
            chunk.transform.rotation = Quaternion.Euler(new Vector3(0, chunk.transform.rotation.eulerAngles.y, 0));
            
            chunk.UpdateSplineImmediately(); //this is required as the chunk is rotated
                
            //get the vertices on each chunks tangent
            Vector3 lastPoint = chunk.LastSample.position;
            lastEndVertices = GetVerticesOnTangent(lastPoint - chunk.LastTangent, lastPoint + chunk.LastTangent);

#if UNITY_EDITOR
            if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                Debug.DrawLine(lastPoint - chunk.LastTangent * 200, lastPoint + chunk.LastTangent * 200, Color.magenta, 15);
#endif
            GlobalLoggers.ChunkLogger.Log($"Found {lastEndVertices.Count} vertices at the end of ({chunk.gameObject.name}) - position = {lastPoint}.");

            Vector3 firstPoint = chunk.FirstSample.position;
            firstEndVertices = GetVerticesOnTangent(firstPoint - chunk.FirstTangent, firstPoint + chunk.FirstTangent);
            
#if UNITY_EDITOR
            if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                Debug.DrawLine(firstPoint - chunk.FirstTangent * 200, firstPoint + chunk.FirstTangent * 200, Color.magenta, 15);
#endif
            GlobalLoggers.ChunkLogger.Log($"Found {firstEndVertices.Count} vertices at the end of ({chunk.gameObject.name}) - position = {lastPoint}.");

            //cache the remaining vertices
            verticesExcludingEnds = new List<int>();
            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                if (lastEndVertices.Contains(vertexIndex) || firstEndVertices.Contains(vertexIndex))
                    continue;
                
                verticesExcludingEnds.Add(vertexIndex);
            }
            
            chunk.transform.rotation = previousRotation;
        }
        
        private List<int> GetVerticesOnTangent(Vector3 tangentStart, Vector3 tangentEnd)
        {
            List<int> verticesOnTangent = new();

            for (var vertexIndex = 0; vertexIndex < Mesh.vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = Mesh.vertices[vertexIndex];
                Vector3 vertexPositionWorld = MeshFilter.transform.TransformPoint(vertexPosition);

#if UNITY_EDITOR
                if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                    Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 10, Color.yellow, 15);
#endif

                if (IsPointOnTangent(vertexPositionWorld, tangentStart, tangentEnd))
                {
                    verticesOnTangent.Add(vertexIndex);
#if UNITY_EDITOR
                    if (chunk.GetComponent<ChunkEditorTools>().ShowDebugLines)
                        Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 20, Color.magenta, 15);
#endif
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