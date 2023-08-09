using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [CustomEditor(typeof(Chunk))]
    public class ChunkEditor : Editor
    {

        private Chunk chunk => target as Chunk;
        
        private void OnSceneGUI()
        {
            chunk.transform.hideFlags = chunk.HasChunkConnected ? HideFlags.NotEditable : HideFlags.None;
        }
        
    }
}
