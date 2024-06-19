#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dreamteck.Splines;
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
            
            SaveEditorAssetsEvents.onSavePrefab -= OnSavePrefab;
            SaveEditorAssetsEvents.onSavePrefab += OnSavePrefab;
        }

        private static void OnSavePrefab(string sceneName, string path)
        {
            RemoveUnusedMeshes(path);
        }

        private static void OnSaveScene(string sceneName, string path)
        {
            RemoveUnusedChunks();
        }

        private static void RemoveUnusedMeshes(string path)
        {
            GameObject chunk = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            string chunkDirectory = $"{ChunkUtils.ChunkMeshAssetFolderPath}/{chunk.GetComponent<Chunk>().UniqueID}";
            
            //find the assets that are used
            List<string> safeFileNames = new List<string>();
            foreach (SplineMesh splineMeshInChunk in chunk.transform.GetComponentsInAllChildren<SplineMesh>())
            {
                string splineMeshAssetName = $"{splineMeshInChunk.gameObject.name}_{splineMeshInChunk.GetComponent<UniqueIDAssigner>().UniqueID}";
                safeFileNames.Add(splineMeshAssetName);
            }
            
            //delete any assets that aren't used in the chunk directory
            string[] filePaths = Directory.GetFiles(chunkDirectory);
            foreach (string filePath in filePaths)
            {
                if (filePath.ContainsAny(safeFileNames))
                    continue; //is safe

                //remove the asset
                AssetDatabase.DeleteAsset(filePath);
            }
        }

        private static void RemoveUnusedChunks()
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