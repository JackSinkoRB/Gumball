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
        [SerializeField, ReadOnly] private List<int> lastEndVertices;
        [SerializeField, ReadOnly] private List<int> firstEndVertices;
        [SerializeField, ReadOnly] private SerializableColor[] vertexColors;
        
        public Chunk Chunk => chunk;
        public MeshFilter MeshFilter => chunk.TerrainHighLOD.GetComponent<MeshFilter>();
        public MeshCollider MeshCollider => chunk.TerrainHighLOD.GetComponent<MeshCollider>();
        public Mesh Mesh => MeshFilter.sharedMesh;
        public Vector3[] Vertices => vertices;
        public SerializableColor[] VertexColors => vertexColors;
        public ReadOnlyCollection<int> LastEndVertices => lastEndVertices.AsReadOnly();
        public ReadOnlyCollection<int> FirstEndVertices => firstEndVertices.AsReadOnly();
        
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
            copy.lastEndVertices = lastEndVertices;
            copy.firstEndVertices = firstEndVertices;
            copy.vertexColors = vertexColors;
            copy.additionalVertexPaintData = additionalVertexPaintData;

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
        
        public void ApplyChanges()
        {
            Mesh meshToUse = Object.Instantiate(Mesh); //use a mesh copy so that we're not editing the actual shared mesh, and so that it can be undone in editor

            meshToUse.SetVertices(vertices);

            //recalculate UVs
            meshToUse.SetUVs(0, ChunkUtils.GetTriplanarUVs(vertices, MeshFilter.transform));

            meshToUse.RecalculateTangents();
            meshToUse.RecalculateNormals();
            meshToUse.RecalculateBounds();
            
            meshToUse.colors = vertexColors.ToColors();

            MeshFilter.sharedMesh = meshToUse;
            MeshCollider.sharedMesh = meshToUse;
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
            Color[] colors = terrainBlendSettings.GetVertexColors(chunk, new List<Vector3>(vertices), MeshFilter.transform, Mesh);

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