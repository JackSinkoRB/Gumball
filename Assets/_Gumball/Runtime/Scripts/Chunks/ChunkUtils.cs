using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using MyBox;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AddressableAssets;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Gumball
{
    public static class ChunkUtils
    {
        
        public enum LoadDirection
        {
            CUSTOM,
            BEFORE,
            AFTER
        }

        public const string TerrainTag = "Terrain";
        public const string ChunkMeshAssetFolderPath = "Assets/_Gumball/Runtime/Meshes/Chunks";
        public const string ChunkFolderPath = "Assets/_Gumball/Runtime/Prefabs/Chunks";
        public const string RuntimeChunkSuffix = "_runtime";
        public const string RuntimeChunksPath = "Assets/_Gumball/Runtime/Prefabs/Chunks/Runtime";

        /// <summary>
        /// Loads the runtime chunk from a chunk reference, or just loads the chunk reference if none exists.
        /// </summary>
        public static AsyncOperationHandle<GameObject> LoadRuntimeChunk(AssetReferenceGameObject chunkReference)
        {
            AsyncOperationHandle<GameObject> handle;
            string runtimeChunkAddress = chunkReference.editorAsset.name + RuntimeChunkSuffix;
            if (AddressableUtils.DoesAddressExist(runtimeChunkAddress)) {
                handle = Addressables.LoadAssetAsync<GameObject>(runtimeChunkAddress);
                handle.WaitForCompletion();
                GlobalLoggers.ChunkLogger.Log($"Found {handle.Result.name} at {runtimeChunkAddress}");
            }
            else
            {
                GlobalLoggers.ChunkLogger.Log($"No runtime chunk at {runtimeChunkAddress}. Loading normal chunk.");
                handle = Addressables.LoadAssetAsync<GameObject>(chunkReference);
            }

            return handle;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Connects the chunks with NEW blend data.
        /// Puts chunk2 at the start or end of chunk1 (depending on direction), and aligns the splines.
        /// <remarks>Should only be used at edit-time. Use pre-generated blend data at runtime.</remarks>
        /// </summary>
        public static ChunkBlendData ConnectChunksWithNewBlendData(Chunk chunk1, Chunk chunk2, LoadDirection direction, bool canUndo = false)
        {
            if (canUndo)
            {
                Undo.RecordObjects(new Object[]
                {
                    chunk1, chunk2,
                    chunk2.transform,
                    chunk1.CurrentTerrain.GetComponent<MeshFilter>(),
                    chunk2.CurrentTerrain.GetComponent<MeshFilter>()
                }, "Connect Chunk");
            }
            
            GlobalLoggers.ChunkLogger.Log($"Appending {chunk2.name} to the end of {chunk1.name}");

            chunk1.DisableAutomaticTerrainRecreation(true);
            chunk2.DisableAutomaticTerrainRecreation(true);

            //update immediately
            chunk1.UpdateSplineImmediately();
            chunk2.UpdateSplineImmediately();

            MoveChunkToOther(chunk1, chunk2, direction);
            
            //update immediately as the position has changed
            chunk2.UpdateSplineImmediately();

            ChunkBlendData blendData = new ChunkBlendData(chunk1, chunk2);

            //update immediately as the position has changed
            chunk1.UpdateSplineImmediately();
            chunk2.UpdateSplineImmediately();
            
            chunk1.GetComponent<ChunkEditorTools>().OnConnectChunkAfter(chunk2);
            chunk2.GetComponent<ChunkEditorTools>().OnConnectChunkBefore(chunk1);
            
            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);

            return blendData;
        }
#endif

        /// <summary>
        /// Connects the chunks using EXISTING blend data.
        /// Puts chunk2 at the start or end of chunk1 (depending on direction), and aligns the splines.
        /// </summary>
        public static void ConnectChunks(Chunk chunk1, Chunk chunk2, LoadDirection direction, ChunkBlendData blendData, bool canUndo = false)
        {
#if UNITY_EDITOR
            if (canUndo)
            {
                Undo.RecordObjects(new Object[]
                {
                    chunk1, chunk2,
                    chunk2.transform,
                    chunk1.CurrentTerrain.GetComponent<MeshFilter>(),
                    chunk2.CurrentTerrain.GetComponent<MeshFilter>()
                }, "Connect Chunk");
            }
#endif
            
            GlobalLoggers.ChunkLogger.Log($"Appending {chunk2.name} to the end of {chunk1.name}");
            
            chunk1.DisableAutomaticTerrainRecreation(true);
            chunk2.DisableAutomaticTerrainRecreation(true);

            chunk1.UpdateSplineSampleData();
            chunk2.UpdateSplineSampleData();

            MoveChunkToOther(chunk1, chunk2, direction);

#if UNITY_EDITOR
            chunk1.GetComponent<ChunkEditorTools>().OnConnectChunkAfter(chunk2);
            chunk2.GetComponent<ChunkEditorTools>().OnConnectChunkBefore(chunk1);
#endif
            
            //update immediately as the position has changed
            chunk2.UpdateSplineImmediately();
            
            chunk1.DisableAutomaticTerrainRecreation(false);
            chunk2.DisableAutomaticTerrainRecreation(false);
        }

        /// <summary>
        /// Gets the UV coordinates from the vertex positions in world space (triplanar mapping).
        /// </summary>
        public static Vector2[] GetTriplanarUVs(List<Vector3> vertexPositions, Transform terrain)
        {
            return GetTriplanarUVs(vertexPositions.ToArray(), terrain);
        }
        
        /// <summary>
        /// Gets the UV coordinates from the vertex positions in world space (triplanar mapping).
        /// </summary>
        public static Vector2[] GetTriplanarUVs(Vector3[] vertexPositions, Transform terrain)
        {
            Vector2[] uvs = new Vector2[vertexPositions.Length];

            for (int vertexIndex = 0; vertexIndex < vertexPositions.Length; vertexIndex++)
            {
                Vector3 vertexPosition = vertexPositions[vertexIndex];
                Vector3 vertexPositionWorld = terrain.TransformPoint(vertexPosition);

                Vector2 uvX = new Vector2(vertexPositionWorld.z, vertexPositionWorld.y);
                Vector2 uvY = new Vector2(vertexPositionWorld.x, vertexPositionWorld.z);
                Vector2 uvZ = new Vector2(vertexPositionWorld.x, vertexPositionWorld.y);

                Vector2 finalUV = uvX + uvY + uvZ;
                uvs[vertexIndex] = finalUV;
            }

            return uvs;
        }

#if UNITY_EDITOR
        public static void BakeMeshes(Chunk chunk, bool replace = true)
        {
            foreach (SplineMesh splineMesh in chunk.SplinesMeshes)
            {
                if (!splineMesh.gameObject.activeSelf)
                    continue;
                
                bool alreadyBaked = splineMesh.baked;
                if (alreadyBaked && !replace)
                    continue;

                splineMesh.Unbake();
                splineMesh.Bake(true, true);

                MeshFilter meshFilter = splineMesh.GetComponent<MeshFilter>();

                string chunkDirectory = $"{ChunkMeshAssetFolderPath}/{chunk.UniqueID}";
                if (!Directory.Exists(chunkDirectory))
                    Directory.CreateDirectory(chunkDirectory);
                string path = $"{chunkDirectory}/{splineMesh.gameObject.name}.asset";
                if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null)
                    AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);

                MeshCollider meshCollider = splineMesh.GetComponent<MeshCollider>();
                Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                meshFilter.sharedMesh = savedMesh;
                meshCollider.sharedMesh = savedMesh;

                PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
                PrefabUtility.RecordPrefabInstancePropertyModifications(meshCollider);
                PrefabUtility.RecordPrefabInstancePropertyModifications(splineMesh);
                EditorUtility.SetDirty(chunk.gameObject);

                AssetDatabase.SaveAssets();

                GlobalLoggers.ChunkLogger.Log("Baked " + path);
            }
        }

        /// <summary>
        /// Gets the desired path for the runtime variation of the chunk.
        /// </summary>
        public static string GetRuntimeChunkPath(GameObject chunk)
        {
            return $"{RuntimeChunksPath}/{chunk.name}{RuntimeChunkSuffix}.prefab";
        }
        
        public static void CreateRuntimeChunk(GameObject originalChunk, string originalChunkPath)
        {
            List<ChunkObject> chunkObjectsInOriginalChunk = originalChunk.transform.GetComponentsInAllChildren<ChunkObject>();
            List<string> assetKeys = new();
            foreach (ChunkObject chunkObject in chunkObjectsInOriginalChunk)
            {
                if (!chunkObject.LoadSeparately)
                    continue;
                
                string assetKey = GameObjectUtils.GetAddressableKeyFromGameObject(chunkObject.gameObject);
                assetKeys.Add(assetKey);
            }

            string newChunkPath = GetRuntimeChunkPath(originalChunk);
            AssetDatabase.CopyAsset(originalChunkPath, newChunkPath);
            GameObject runtimePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newChunkPath);
            GameObject runtimePrefabInstance = Object.Instantiate(runtimePrefab);

            runtimePrefabInstance.GetComponent<UniqueIDAssigner>().SetPersistent(true);
            
            //find all chunk object references and save the data
            // - then destroy all the chunk objects
            List<ChunkObject> chunkObjects = runtimePrefabInstance.transform.GetComponentsInAllChildren<ChunkObject>();
            List<ChunkObjectData> chunkObjectData = new();

            for (int index = 0; index < chunkObjects.Count; index++)
            {
                ChunkObject chunkObject = chunkObjects[index];
                
                if (!chunkObject.LoadSeparately)
                    continue;

                chunkObject.transform.SetParent(runtimePrefabInstance.transform);
                chunkObjectData.Add(new ChunkObjectData(assetKeys[index], chunkObject.transform.localPosition, chunkObject.transform.localRotation, chunkObject.transform.localScale));
                Object.DestroyImmediate(chunkObject.gameObject);
            }

            //need to reattach the meshes as the references get lost:
            SplineMesh[] meshes = runtimePrefabInstance.GetComponent<Chunk>().SplinesMeshes;
            for (int index = 0; index < meshes.Length; index++)
            {
                SplineMesh splineMesh = meshes[index];
                splineMesh.GetComponent<MeshFilter>().sharedMesh = originalChunk.GetComponent<Chunk>().SplinesMeshes[index].GetComponent<MeshFilter>().sharedMesh;
            }

            //update the data
            runtimePrefabInstance.GetComponent<Chunk>().SetChunkObjectData(chunkObjectData.ToArray());

            PrefabUtility.SaveAsPrefabAsset(runtimePrefabInstance, newChunkPath);

            //dispose of instance
            Object.DestroyImmediate(runtimePrefabInstance);
            
            //replace the addressable asset at the original path with this asset
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            
            const string groupName = "RuntimeChunks";
            AddressableAssetGroup group = settings.FindGroup(groupName);
            string guid = AssetDatabase.AssetPathToGUID(newChunkPath);
            
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = $"{Path.GetFileNameWithoutExtension(originalChunkPath)}{RuntimeChunkSuffix}";
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
        }
#endif
        
        private static void MoveChunkToOther(Chunk chunk1, Chunk chunk2, LoadDirection direction)
        {
            //align the rotation of chunk2 to match chunk1
            Quaternion firstSplineEndRotation = direction == LoadDirection.AFTER ? chunk1.LastSample.rotation : chunk1.FirstSample.rotation;
            Quaternion secondSplineStartRotation = direction == LoadDirection.AFTER ? chunk2.FirstSample.rotation : chunk2.LastSample.rotation;
            Quaternion rotationDifference = firstSplineEndRotation * Quaternion.Inverse(secondSplineStartRotation);
            chunk2.transform.rotation *= rotationDifference;
            
            //update immediately
            chunk2.UpdateSplineImmediately();

            //set the position of chunk 2 to the end of chunk 1
            SplineSample chunk2ConnectionPoint = direction == LoadDirection.AFTER ? chunk2.FirstSample : chunk2.LastSample;
            SplineSample chunk1ConnectionPoint = direction == LoadDirection.AFTER ? chunk1.LastSample : chunk1.FirstSample;
            chunk2.transform.position += chunk1ConnectionPoint.position - chunk2ConnectionPoint.position;
        }
        
    }
}
