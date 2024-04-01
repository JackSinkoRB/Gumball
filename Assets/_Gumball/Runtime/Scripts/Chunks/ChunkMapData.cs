using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct ChunkMapData
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;
        [SerializeField] private ChunkMeshData finalMeshData;
            
        [SerializeField] private bool hasCustomLoadDistance;
        [SerializeField] private float customLoadDistance;
        [SerializeField] private Vector3 splineStartPosition;
        [SerializeField] private Vector3 splineEndPosition;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private float splineLength;
        
        public Vector3 Position => position;
        public Quaternion Rotation => rotation;
        public ChunkMeshData FinalMeshData => finalMeshData;

        public bool HasCustomLoadDistance => hasCustomLoadDistance;
        public float CustomLoadDistance => customLoadDistance;
        public Vector3 SplineStartPosition => splineStartPosition;
        public Vector3 SplineEndPosition => splineEndPosition;
        public float SplineLength => splineLength;
        
        public ChunkMapData(Chunk originalChunk, ChunkMeshData finalMeshData)
        {
            position = originalChunk.transform.position;
            rotation = originalChunk.transform.rotation;
            this.finalMeshData = finalMeshData;
                
            hasCustomLoadDistance = originalChunk.HasCustomLoadDistance;
            customLoadDistance = originalChunk.CustomLoadDistance;
                
            originalChunk.UpdateSplineSampleData();
            splineStartPosition = originalChunk.FirstSample.position;
            splineEndPosition = originalChunk.LastSample.position;
            splineLength = originalChunk.SplineLengthCached;
        }

        public void ApplyToChunk(Chunk chunk)
        {
            chunk.UpdateSplineImmediately();
            chunk.transform.position = Position;
            chunk.transform.rotation = Rotation;
            chunk.ChunkMeshData.SetVertices(FinalMeshData.Vertices);
            chunk.ChunkMeshData.UpdateVertexColors(FinalMeshData.VertexColors);
            chunk.ChunkMeshData.ApplyChanges();
            chunk.UpdateSplineImmediately();
        }
    }
}
