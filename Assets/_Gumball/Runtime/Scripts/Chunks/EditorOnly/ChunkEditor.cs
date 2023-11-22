#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [CustomEditor(typeof(ChunkEditorTools))]
    public class ChunkEditor : UnityEditor.Editor
    {

        private ChunkEditorTools chunk => target as ChunkEditorTools;

        private void OnSceneGUI()
        {
            chunk.transform.hideFlags = chunk.HasChunkConnected ? HideFlags.NotEditable : HideFlags.None;
            
            if (!chunk.HasChunkConnected && !chunk.transform.rotation.eulerAngles.Approximately(Vector3.zero))
                chunk.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        
    }
}
#endif