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
        [SerializeField] private string addressableKey;
        [SerializeField] private int mapIndex;

        public Chunk Chunk => chunk;
        public string AddressableKey => addressableKey;
        public int MapIndex => mapIndex;
            
        public LoadedChunkData(Chunk chunk, string addressableKey, int mapIndex)
        {
            this.chunk = chunk;
            this.addressableKey = addressableKey;
            this.mapIndex = mapIndex;
        }
    }
}
