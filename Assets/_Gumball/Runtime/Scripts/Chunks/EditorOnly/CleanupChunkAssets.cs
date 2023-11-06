#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
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

        private static void OnSaveScene(string sceneName)
        {
            CleanupUnusedMeshes(sceneName);
        }

        private static void CleanupUnusedMeshes(string sceneName)
        {
            if (Application.isPlaying)
                return;

            CleanupUnusedMeshes(sceneName, ChunkUtils.TerrainMeshAssetFolderPath, ChunkUtils.TerrainMeshPrefix);
            CleanupUnusedMeshes(sceneName, ChunkUtils.RoadMeshAssetFolderPath, ChunkUtils.RoadMeshPrefix);
            
            AssetDatabase.SaveAssets();
        }

        private static void CleanupUnusedMeshes(string sceneName, string meshFolderPath, string filePrefix)
        {
            //find all the used meshes
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { ChunkUtils.ChunkFolderPath });
            HashSet<string> whitelistedIds = new HashSet<string>();
            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                Chunk chunkPrefab = AssetDatabase.LoadAssetAtPath<Chunk>(prefabPath);
                if (chunkPrefab == null)
                    continue; 
                if (chunkPrefab.CurrentTerrain == null)
                    continue;

                whitelistedIds.Add(chunkPrefab.UniqueID);
            }

            //find all the terrain meshes
            string[] meshGuids = AssetDatabase.FindAssets("t:Mesh", new[] { meshFolderPath });
            foreach (string meshGuid in meshGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(meshGuid);
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                bool isWhitelisted = false;
                foreach (string whitelistedId in whitelistedIds)
                {
                    if (assetPath.Contains(whitelistedId))
                    {
                        isWhitelisted = true;
                        break;
                    }
                }

                //only delete scene is the one being saved
                bool isApartOfScene = assetPath.Replace(filePrefix, "").Contains("_") && !assetPath.Contains("Prefab");
                bool isApartOfCurrentScene = assetPath.Contains(sceneName);
                if (isApartOfScene)
                {
                    if (isApartOfCurrentScene)
                    {
                        //search all gameobjects for chunk components
                        List<Chunk> chunksInScene = SceneUtils.GetAllComponentsInScene<Chunk>(sceneName);
                        foreach (Chunk chunkInScene in chunksInScene)
                        {
                            if (assetPath.Contains(chunkInScene.UniqueID))
                            {
                                isWhitelisted = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isWhitelisted = true;
                    }
                }

                if (!isWhitelisted)
                {
                    //remove it
                    string path = AssetDatabase.GetAssetPath(mesh);
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log($"Removed unused mesh asset: {path}");
                }
            }
        }

    }
}
#endif