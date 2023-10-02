using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Serializable]
    public struct ChunkPair : IEquatable<ChunkPair>
    {
        private AssetReferenceGameObject firstChunk;
        private AssetReferenceGameObject lastChunk;

        public ChunkPair(AssetReferenceGameObject firstChunk, AssetReferenceGameObject lastChunk)
        {
            this.firstChunk = firstChunk;
            this.lastChunk = lastChunk;
        }

        public bool Equals(ChunkPair other)
        {
            if (firstChunk.RuntimeKey.Equals(other.firstChunk.RuntimeKey) && lastChunk.RuntimeKey.Equals(other.lastChunk.RuntimeKey))
                return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkPair other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(firstChunk.RuntimeKey, lastChunk.RuntimeKey);
        }
    }
}
