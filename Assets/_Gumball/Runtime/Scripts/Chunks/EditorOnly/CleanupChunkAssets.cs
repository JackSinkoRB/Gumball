#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dreamteck.Splines;
using Gumball.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
            RemoveUnusedSplineMeshes(path);
        }

        private static void OnSaveScene(string sceneName, string path)
        {
            RemoveUnusedChunks();
        }

        private static void RemoveUnusedSplineMeshes(string path)
        {
            GameObject chunkAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (chunkAsset == null)
                return; //no asset at path

            Chunk chunk = chunkAsset.GetComponent<Chunk>();
            if (chunk == null)
                return; //not a chunk asset
            
            string chunkDirectory = $"{ChunkUtils.ChunkMeshAssetFolderPath}/{chunk.UniqueID}";

            if (!Directory.Exists(chunkDirectory))
                return; //nothing to delete
            
            //find the assets that are used
            List<string> safeFileNames = new List<string>();
            foreach (SplineMesh splineMeshInChunk in chunkAsset.transform.GetComponentsInAllChildren<SplineMesh>())
            {
                string splineMeshAssetName = $"{splineMeshInChunk.gameObject.name}_{chunkAsset.transform.InverseTransformPoint(splineMeshInChunk.transform.position.Round(1))}";
                safeFileNames.Add(splineMeshAssetName);
            }
            
            //ignore terrain
            foreach (Chunk.TerrainLOD lod in Enum.GetValues(typeof(Chunk.TerrainLOD)))
                safeFileNames.Add($"Terrain-{lod.ToString()}");
            
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