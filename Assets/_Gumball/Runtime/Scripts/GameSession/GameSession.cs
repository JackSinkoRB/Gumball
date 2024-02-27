using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public abstract class GameSession : ScriptableObject
    {
        
        [SerializeField] private ChunkMap chunkMap;

        public ChunkMap ChunkMap => chunkMap;
        public string Type => GetType().Name.Replace("GameSession", "");
        
    }
}
