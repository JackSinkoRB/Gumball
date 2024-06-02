using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        public const string RuntimeChunksPath = "Assets/_Gumball/Runtime/Prefabs/Chunks/_Runtime";
        
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
                    chunk1.TerrainHighLOD.GetComponent<MeshFilter>(),
                    chunk2.TerrainHighLOD.GetComponent<MeshFilter>()
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
                    chunk1.TerrainHighLOD.GetComponent<MeshFilter>(),
                    chunk2.TerrainHighLOD.GetComponent<MeshFilter>()
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
        
        /// <returns>True if the object was grounded, or false if it failed.</returns>
        public static bool GroundObject(Transform transform)
        {
            float offset = 0;
            bool hit = false;
            
            Vector3 originalPosition = transform.position;
            try
            {
                transform.position = transform.position.OffsetY(10000);

                hit = transform.gameObject.scene.GetPhysicsScene().Raycast(transform.position, Vector3.down, out RaycastHit hitDown, Mathf.Infinity, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.Terrain));
                if (hit)
                    offset = -hitDown.distance;
            }
            finally
            {
                transform.position = hit && offset != 0 ? transform.position.OffsetY(offset) : originalPosition;
            }

            return hit;
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
        
        /// <summary>
        /// Creates a runtime version of the original chunk that is stripped of chunk objects.
        /// </summary>
        /// <returns>The addressable runtime key for the runtime chunk.</returns>
        public static string CreateRuntimeChunk(GameObject originalChunk, bool saveAssetsOnComplete = true)
        {
            if (originalChunk.name.Contains(RuntimeChunkSuffix))
            {
                Debug.LogError($"Cannot create runtime chunk from another runtime chunk ({originalChunk.name}).");
                return null;
            }
            
            List<ChunkObject> chunkObjectsInOriginalChunk = originalChunk.transform.GetComponentsInAllChildren<ChunkObject>();
            List<string> assetKeys = new();
            foreach (ChunkObject chunkObject in chunkObjectsInOriginalChunk)
            {
                string assetKey = GameObjectUtils.GetAddressableKeyFromGameObject(chunkObject.gameObject, false);
                assetKeys.Add(assetKey);
            }

            //update spline meshes in original chunk in case they haven't had a chunk to save
            originalChunk.GetComponent<Chunk>().FindSplineMeshes();

            string newChunkPath = GetRuntimeChunkPath(originalChunk);
            GameObject runtimePrefabInstance = Object.Instantiate(originalChunk);

            runtimePrefabInstance.GetComponent<UniqueIDAssigner>().SetPersistent(true);
            
            //find all chunk object references and save the data
            // - then destroy all the chunk objects
            List<ChunkObject> chunkObjects = runtimePrefabInstance.transform.GetComponentsInAllChildren<ChunkObject>();
            Dictionary<string, List<ChunkObjectData>> chunkObjectData = new();
            
            for (int index = 0; index < chunkObjects.Count; index++)
            {
                ChunkObject chunkObject = chunkObjects[index];

                if (chunkObject == null || chunkObject.IsChildOfAnotherChunkObject)
                {
                    Debug.LogWarning($"Chunk object at index {index} could not be saved as it is a child of another chunk object that was removed.");
                    continue;
                }

                if (!chunkObject.isActiveAndEnabled)
                    continue;
                
                if (chunkObject.IgnoreAtRuntime)
                {
                    Object.DestroyImmediate(chunkObject.gameObject);
                    continue;
                }
                
                if (!chunkObject.LoadSeparately)
                    continue;
                
                string assetKey = assetKeys[index];
                if (assetKey == null)
                {
                    Debug.LogError($"Asset key was null for index {index} ({chunkObject.gameObject.name}. It won't be shown at runtime. Is it a prefab asset ending in .prefab?");
                    Object.DestroyImmediate(chunkObject.gameObject);
                    continue;
                }
                
                List<ChunkObjectData> chunkObjectList = chunkObjectData.ContainsKey(assetKey) ? chunkObjectData[assetKey] : new List<ChunkObjectData>();
                ChunkObjectData data = new ChunkObjectData(originalChunk.GetComponent<Chunk>(), chunkObject);
                chunkObjectList.Add(data);
                chunkObjectData[assetKey] = chunkObjectList;

                Object.DestroyImmediate(chunkObject.gameObject);
            }

            //need to reattach the meshes as the references get lost:
            SplineMesh[] meshes = runtimePrefabInstance.GetComponent<Chunk>().SplinesMeshes;
            for (int index = 0; index < meshes.Length; index++)
            {
                SplineMesh splineMesh = meshes[index];
                
                Mesh originalMesh = originalChunk.GetComponent<Chunk>().SplinesMeshes[index].GetComponent<MeshFilter>().sharedMesh;
                splineMesh.GetComponent<MeshFilter>().sharedMesh = originalMesh;
            }

            //delete empty gameobjects
            HashSet<GameObject> emptyObjects = new();
            foreach (Transform child in runtimePrefabInstance.transform)
            {
                if (child.gameObject.IsCompletelyEmpty())
                    emptyObjects.Add(child.gameObject);
            }
            foreach (GameObject emptyGameObject in emptyObjects)
            {
                Object.DestroyImmediate(emptyGameObject);
            }

            //show error if there's a large amount of objects (suggesting to use ChunkObjects)
            const int maxChildrenBeforeError = 25;
            int totalChildren = runtimePrefabInstance.GetTotalChildCount();
            if (totalChildren > maxChildrenBeforeError)
                Debug.LogError($"{runtimePrefabInstance.name} has a large amount of children ({totalChildren}) in the runtime chunk. Could any objects be setup as ChunkObjects and loaded separately?");
            
            //create raycast detector object
            runtimePrefabInstance.GetComponent<Chunk>().TryCreateChunkDetector();
            
            //update the data
            runtimePrefabInstance.GetComponent<Chunk>().SetChunkObjectData(chunkObjectData);

            //calculate the spline length
            runtimePrefabInstance.GetComponent<Chunk>().CalculateSplineLength();
            
            PrefabUtility.SaveAsPrefabAsset(runtimePrefabInstance, newChunkPath);

            //dispose of instance
            Object.DestroyImmediate(runtimePrefabInstance);
            
            //set the asset as addressable
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            
            const string groupName = "RuntimeChunks";
            AddressableAssetGroup group = settings.FindGroup(groupName);
            string guid = AssetDatabase.AssetPathToGUID(newChunkPath);
            
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = $"{originalChunk.name}{RuntimeChunkSuffix}";
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            
            if (saveAssetsOnComplete)
                AssetDatabase.SaveAssets();
            
            return entry.address;
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
