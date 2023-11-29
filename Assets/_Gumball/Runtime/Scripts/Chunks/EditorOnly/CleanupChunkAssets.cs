#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gumball.Editor;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    public static class CleanupChunkAssets
    {

        [InitializeOnLoadMethod]
        private static void Initialise()
        {
            SaveEditorAssetsEvents.onSaveScene -= OnSaveScene;
            SaveEditorAssetsEvents.onSaveScene += OnSaveScene;
        }

        private static void OnSaveScene(string sceneName, string path)
        {
            CleanupUnusedMeshes();
        }

        private static void CleanupUnusedMeshes()
        {
            if (Application.isPlaying)
                return;

            HashSet<string> whiteListedChunks = new HashSet<string>();
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ChunkUtils.ChunkFolderPath });
            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                Chunk chunkPrefab = AssetDatabase.LoadAssetAtPath<Chunk>(prefabPath);
                if (chunkPrefab == null)
                    continue;

                whiteListedChunks.Add(chunkPrefab.UniqueID);
            }
            
            string[] chunksWithMeshes = Directory.GetDirectories(ChunkUtils.ChunkMeshAssetFolderPath);
            
            foreach (string chunkPath in chunksWithMeshes)
            {
                string chunkID = Path.GetFileNameWithoutExtension(chunkPath);
                if (!whiteListedChunks.Contains(chunkID))
                {
                    AssetDatabase.DeleteAsset(chunkPath);
                    Debug.Log($"Removed unused mesh assets for {chunkID}.");
                }
            }
            
            AssetDatabase.SaveAssets();
        }

    }
}
#endif