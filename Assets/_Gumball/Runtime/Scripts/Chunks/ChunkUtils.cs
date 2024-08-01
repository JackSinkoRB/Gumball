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
using Debug = UnityEngine.Debug;
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

        private struct ChunkObjectHandling
        {
            public readonly bool CanDestroy;
            public readonly bool CanSaveData;
            public readonly string AssetKey;

            public ChunkObjectHandling(bool destroy, bool saveData, string assetKey)
            {
                CanDestroy = destroy;
                CanSaveData = saveData;
                AssetKey = assetKey;
            }
        }
        
        public const string TerrainTag = "Terrain";
        public const string ChunkMeshAssetFolderPath = "Assets/_Gumball/Runtime/Meshes/Chunks";
        public const string ChunkFolderPath = "Assets/_Gumball/Runtime/Prefabs/Chunks";
        public const string RuntimeChunkSuffix = "_runtime";
        public const string RuntimeChunksPath = "Assets/_Gumball/Runtime/Prefabs/Chunks/_Runtime";
        
        private const string chunkObjectAddressableGroup = "ChunkObjects";
        private const string chunkObjectAddressableSuffix = "_ChunkObject";
        
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
        public static void BakeMeshes(Chunk chunk)
        {
            SplineMesh[] splineMeshes = chunk.transform.GetComponentsInAllChildren<SplineMesh>().ToArray();
            foreach (SplineMesh splineMesh in splineMeshes)
            {
                if (splineMesh == null || !splineMesh.gameObject.activeSelf)
                    continue;

                string chunkDirectory = $"{ChunkMeshAssetFolderPath}/{chunk.UniqueID}";
                if (!Directory.Exists(chunkDirectory))
                    Directory.CreateDirectory(chunkDirectory);

                UniqueIDAssigner uniqueIDAssigner = splineMesh.GetComponent<UniqueIDAssigner>();
                if (uniqueIDAssigner == null)
                    continue;

                splineMesh.Unbake();
                if (splineMesh.GetChannelCount() > 0)
                    splineMesh.Bake(true, true);
                
                string path = $"{chunkDirectory}/{splineMesh.gameObject.name}_{uniqueIDAssigner.UniqueID}.asset";
                Mesh existingAsset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existingAsset != null)
                    AssetDatabase.DeleteAsset(path);
                
                MeshFilter meshFilter = splineMesh.GetComponent<MeshFilter>();
                AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);
                AssetDatabase.SaveAssets();

                MeshCollider meshCollider = splineMesh.GetComponent<MeshCollider>();
                Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = savedMesh;
                    EditorUtility.SetDirty(meshFilter);
                }

                if (meshCollider != null)
                {
                    meshCollider.sharedMesh = savedMesh;
                    EditorUtility.SetDirty(meshCollider);
                }
                
                EditorUtility.SetDirty(splineMesh);
                EditorUtility.SetDirty(chunk.gameObject);
                
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

        private static ChunkObjectHandling GetChunkObjectHandling(ChunkObject chunkObject)
        {
            if (chunkObject == null)
                return new ChunkObjectHandling(false, false, null);
            
            if (chunkObject.IsChildOfAnotherChunkObject)
            {
                Debug.LogWarning($"Chunk object {chunkObject.gameObject.name} will not be treated as a chunk object, as it is a child of another chunk object.");
                return new ChunkObjectHandling(true, false, null);
            }
            
            if (chunkObject.IgnoreAtRuntime || !chunkObject.gameObject.activeSelf || !chunkObject.enabled)
                return new ChunkObjectHandling(true, false, null);

            //check if there's at least 1 mesh renderer - otherwise just delete it - may have been removed after combining meshes
            bool nothingToRender = chunkObject.GetComponent<MeshRenderer>() == null && chunkObject.transform.GetComponentsInAllChildren<MeshRenderer>().Count == 0;
            if (nothingToRender)
                return new ChunkObjectHandling(true, false, null);
            
            if (!chunkObject.LoadSeparately)
                return new ChunkObjectHandling(false, false, null); //not destroying
            
            string assetKey = GameObjectUtils.GetOrSetAddressableKeyFromGameObject(chunkObject.gameObject, chunkObjectAddressableGroup, chunkObjectAddressableSuffix, false);
            if (assetKey == null)
            {
                Debug.LogError($"Asset key was null for {chunkObject.gameObject.name}, therefore it won't be treated as a ChunkObject. Is it a prefab asset ending in .prefab?");
                return new ChunkObjectHandling(false, false, null);
            }
            
            return new ChunkObjectHandling(true, true, assetKey);
        }
        
        /// <summary>
        /// Creates a runtime version of the original chunk that is stripped of chunk objects.
        /// </summary>
        /// <returns>The addressable runtime key for the runtime chunk.</returns>
        public static string CreateRuntimeChunk(GameObject prefab, GameObject instanceToCopy = null, bool saveAssetsOnComplete = true)
        {
            if (prefab.name.Contains(RuntimeChunkSuffix))
            {
                Debug.LogError($"Cannot create runtime chunk from another runtime chunk ({prefab.name}).");
                return null;
            }

            GameObject chunk = instanceToCopy == null ? prefab : instanceToCopy;
            chunk.GetComponent<ChunkEditorTools>().CheckToAssignSplineMeshIDs();
            
            //create runtime chunk
            GameObject runtimeInstance = Object.Instantiate(chunk);
            
            Chunk runtimeInstanceChunk = runtimeInstance.GetComponent<Chunk>();
            runtimeInstance.GetComponent<UniqueIDAssigner>().SetPersistent(true);

            List<ChunkObject> chunkObjectsInPrefab = prefab.transform.GetComponentsInAllChildren<ChunkObject>();
            List<ChunkObject> chunkObjectsInInstance = runtimeInstance.transform.GetComponentsInAllChildren<ChunkObject>();

            //check to remove any chunk objects
            for (int index = 0; index < chunkObjectsInPrefab.Count; index++)
            {
                ChunkObject chunkObjectInPrefab = chunkObjectsInPrefab[index];
                ChunkObject chunkObjectInInstance = chunkObjectsInInstance[index];

                if (GetChunkObjectHandling(chunkObjectInPrefab).CanDestroy
                    && chunkObjectInInstance != null) //may have already been destroyed
                {
                    Object.DestroyImmediate(chunkObjectInInstance.gameObject);
                }
            }
            
            //bake spline meshes
            BakeMeshes(runtimeInstanceChunk);

            //delete empty gameobjects
            HashSet<GameObject> emptyObjects = new();
            foreach (Transform child in runtimeInstance.transform)
            {
                if (child.gameObject.IsCompletelyEmpty())
                    emptyObjects.Add(child.gameObject);
            }
            foreach (GameObject emptyGameObject in emptyObjects)
            {
                Object.DestroyImmediate(emptyGameObject);
            }

            //show error if there's a large amount of objects (suggesting to use ChunkObjects)
            const int maxChildrenBeforeError = 50;
            int totalChildren = runtimeInstance.GetTotalChildCount();
            if (totalChildren > maxChildrenBeforeError)
                Debug.LogWarning($"{runtimeInstance.name.Replace("(Clone)", "")} has a large amount of children ({totalChildren}) in the runtime chunk. Could any objects be setup as ChunkObjects and loaded separately?");
            
            //create raycast detector object
            runtimeInstanceChunk.TryCreateChunkDetector();

            //calculate the spline length
            runtimeInstanceChunk.CalculateSplineLength();
            
            //save the runtime chunk asset
            string runtimeChunkPath = GetRuntimeChunkPath(prefab);
            PrefabUtility.SaveAsPrefabAsset(runtimeInstance, runtimeChunkPath);
            
            //dispose of instance
            Object.DestroyImmediate(runtimeInstance);
            
            //set the asset as addressable
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            
            const string groupName = "RuntimeChunks";
            AddressableAssetGroup group = settings.FindGroup(groupName);
            string guid = AssetDatabase.AssetPathToGUID(runtimeChunkPath);
            
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = $"{prefab.name}{RuntimeChunkSuffix}";
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            
            if (saveAssetsOnComplete)
                AssetDatabase.SaveAssets();
            
            return entry.address;
        }

        public static Dictionary<string, List<ChunkObjectData>> CreateChunkObjectData(GameObject chunkPrefab, Chunk chunkInstance)
        {
            Dictionary<string, List<ChunkObjectData>> chunkObjectData = new();
            
            //find all chunk objects and save the data
            List<ChunkObject> chunkObjectsFromPrefab = chunkPrefab.transform.GetComponentsInAllChildren<ChunkObject>();
            List<ChunkObject> chunkObjectsFromInstance = chunkInstance.transform.GetComponentsInAllChildren<ChunkObject>();

            for (int index = 0; index < chunkObjectsFromPrefab.Count; index++)
            {
                ChunkObject chunkObjectFromPrefab = chunkObjectsFromPrefab[index];
                ChunkObject chunkObjectFromInstance = chunkObjectsFromInstance[index];
                
                ChunkObjectHandling chunkObjectHandling = GetChunkObjectHandling(chunkObjectFromPrefab);
                if (!chunkObjectHandling.CanSaveData)
                    continue;

                //make sure chunk objects have updated (eg. applied grounded, distance from road spline)
                chunkObjectFromInstance.UpdatePosition();

                string assetKey = chunkObjectHandling.AssetKey;

                List<ChunkObjectData> chunkObjectList = chunkObjectData.ContainsKey(assetKey) ? chunkObjectData[assetKey] : new List<ChunkObjectData>();
                ChunkObjectData data = new ChunkObjectData(chunkInstance, chunkObjectFromInstance);
                chunkObjectList.Add(data);
                chunkObjectData[assetKey] = chunkObjectList;
            }

            return chunkObjectData;
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
