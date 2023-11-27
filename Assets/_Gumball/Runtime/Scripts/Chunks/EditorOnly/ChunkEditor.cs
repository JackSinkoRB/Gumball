#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [CustomEditor(typeof(Chunk))]
    public class ChunkEditor : UnityEditor.Editor
    {

        private Chunk chunk => target as Chunk;
        private ChunkEditorTools chunkEditorTools => chunk.GetComponent<ChunkEditorTools>();
        
        private void OnSceneGUI()
        {
            chunk.transform.hideFlags = chunkEditorTools.HasChunkConnected ? HideFlags.NotEditable : HideFlags.None;
            
            if (!chunkEditorTools.HasChunkConnected && !chunk.transform.rotation.eulerAngles.Approximately(Vector3.zero))
                chunk.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        
    }
}
#endif