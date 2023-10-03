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
    public class ChunkEditor : Editor
    {

        private Chunk chunk => target as Chunk;
        
        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (target == null || chunk.gameObject == null)
            {
                OnDestroyedInEditor();
            }
        }

        private void OnDestroyedInEditor()
        {
            ChunkUtils.CleanupUnusedMeshes();
        }

        private void OnSceneGUI()
        {
            chunk.transform.hideFlags = chunk.HasChunkConnected ? HideFlags.NotEditable : HideFlags.None;
            
            if (!chunk.HasChunkConnected && !chunk.transform.rotation.eulerAngles.Approximately(Vector3.zero))
                chunk.transform.rotation = Quaternion.Euler(Vector3.zero);
            
            chunk.GetComponent<ChunkEditorTools>().HasConnection = chunk.HasChunkConnected;
        }
        
    }
}
#endif