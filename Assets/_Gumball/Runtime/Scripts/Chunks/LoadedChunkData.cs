using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Serializable]
    public struct LoadedChunkData
    {
        [SerializeField] private Chunk chunk;
        [SerializeField] private AssetReferenceGameObject assetReference;
        [SerializeField] private int mapIndex;

        public Chunk Chunk => chunk;
        public AssetReferenceGameObject AssetReference => assetReference;
        public int MapIndex => mapIndex;
            
        public LoadedChunkData(Chunk chunk, AssetReferenceGameObject assetReference, int mapIndex)
        {
            this.chunk = chunk;
            this.assetReference = assetReference;
            this.mapIndex = mapIndex;
        }
    }
}
