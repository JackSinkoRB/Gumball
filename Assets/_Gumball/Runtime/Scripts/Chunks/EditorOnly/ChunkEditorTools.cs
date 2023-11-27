using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Gumball.Editor;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gumball
{
    [ExecuteAlways]
    public class ChunkEditorTools : MonoBehaviour
    {

#if UNITY_EDITOR
        private Chunk chunk => GetComponent<Chunk>();
        private float timeSinceUnityUpdated => Time.realtimeSinceStartup - timeWhenUnityLastUpdated;

        [ReadOnly, SerializeField] private Chunk chunkBefore;
        [ReadOnly, SerializeField] private Chunk chunkAfter;

        [SerializeField, HideInInspector] private bool hasChunkConnected;

        public bool HasChunkConnected => hasChunkConnected;
        
        private GameObject previousSelection;
        private float timeWhenUnityLastUpdated;

        private void OnSavePrefab(string prefabName, string path)
        {
            if (prefabName.Equals(gameObject.name))
            {
                ChunkUtils.BakeMeshes(chunk);
            }
        }
        
        private void OnEnable()
        {
            SaveEditorAssetsEvents.onSavePrefab += OnSavePrefab;
            
            chunk.SplineComputer.onRebuild += CheckToUpdateMeshesImmediately;
            chunk.UpdateSplineSampleData();
        }

        private void OnDisable()
        {
            chunk.SplineComputer.onRebuild -= CheckToUpdateMeshesImmediately;

            SaveEditorAssetsEvents.onSavePrefab -= OnSavePrefab;
            
            Tools.hidden = false;
        }

        private void OnDrawGizmos()
        {
            currentGrid?.OnDrawGizmos();
            CheckToShowVertices();
        }

        private void Update()
        {
            CheckIfJustDeselected();
            
            previousSelection = Selection.activeGameObject;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                timeWhenUnityLastUpdated = Time.realtimeSinceStartup;
        }
        
        private void LateUpdate()
        {
            CheckToDisableTools();
        }
        
        private void OnValidate()
        {
            CheckToUpdateMeshesImmediately();
            CheckIfTerrainIsRaycastable();
        }

        private void CheckIfTerrainIsRaycastable()
        {
            if (chunk.CurrentTerrain == null)
                return;

            chunk.CurrentTerrain.layer = (int)GameObjectLayers.Layer.Terrain;
            chunk.CurrentTerrain.GetComponent<MeshCollider>(true);
        }

        #region Show terrain vertices
        private const float timeToShowVertices = 15;
        
        private float timeLastClickedShowTerrainVertices = -10000f;
        
        private float timeSinceClickedShowTerrainVertices => Time.realtimeSinceStartup - timeLastClickedShowTerrainVertices;
        
        [ButtonMethod]
        public void ShowTerrainVertices()
        {
            timeLastClickedShowTerrainVertices = Time.realtimeSinceStartup;
        }
        
        private void CheckToShowVertices()
        {
            if (timeSinceClickedShowTerrainVertices > timeToShowVertices)
                return;

            MeshFilter meshFilter = chunk.CurrentTerrain.GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;

            if (meshFilter.sharedMesh == null)
                return;
            
            Vector3[] vertices = meshFilter.sharedMesh.vertices;
            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                Vector3 vertexPosition = vertices[vertexIndex];
                Vector3 vertexPositionWorld = meshFilter.transform.TransformPoint(vertexPosition);
                
                Debug.DrawLine(vertexPositionWorld, vertexPositionWorld + Vector3.up * 20, Color.blue);
                Handles.Label(vertexPositionWorld, vertexIndex.ToString());
            }
        }
        #endregion
        
        #region Generate terrain

        [Header("Create terrain")]
        [ReadOnly(nameof(hasChunkConnected)), SerializeField]
        private ChunkTerrainData terrainData = new();
        [Tooltip("If enabled, the terrain will update whenever a value is changed. Otherwise the CreateTerrain button will need to be used.")]
        [SerializeField] private bool updateImmediately = true;

        [Header("Blending")]
        [PositiveValueOnly, SerializeField] private float terrainBlendDistance = 50;
        public float TerrainBlendDistance => terrainBlendDistance;

        private ChunkGrid currentGrid;
        
        private static bool subscribedToPlayModeStateChanged;
        private static PlayModeStateChange playModeState;
        
        [ButtonMethod]
        public void ShowTerrainGrid()
        {
            currentGrid = new ChunkGrid(chunk, terrainData.Resolution, terrainData.WidthAroundRoad, true);
        }
        
        public void ShowEndsOfSpline()
        {
            chunk.SplineComputer.RebuildImmediate();
            chunk.UpdateSplineSampleData();
            Debug.DrawRay(chunk.FirstSample.position, chunk.FirstSample.forward * 15, Color.blue, 15);
            Debug.DrawRay(chunk.FirstSample.position, chunk.FirstSample.up * 15, Color.green, 15);
            Debug.DrawRay(chunk.FirstSample.position, chunk.FirstSample.right * 15, Color.red, 15);
            
            Debug.DrawRay(chunk.LastSample.position, chunk.LastSample.forward * 15, Color.blue, 15);
            Debug.DrawRay(chunk.LastSample.position, chunk.LastSample.up * 15, Color.green, 15);
            Debug.DrawRay(chunk.LastSample.position, chunk.LastSample.right * 15, Color.red, 15);
        }
        
        [ButtonMethod()]
        public void CreateTerrain()
        {
            GameObject newTerrain = terrainData.Create(chunk);
            chunk.SetTerrain(newTerrain);
            currentGrid = terrainData.Grid;
            
            Selection.SetActiveObjectWithContext(newTerrain, chunk);
            Undo.RegisterCreatedObjectUndo(newTerrain, "Create Terrain");
        }
        
        [InitializeOnLoadMethod]
        private static void SubscribeToPlayModeStateChanged()
        {
            if (!subscribedToPlayModeStateChanged)
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                subscribedToPlayModeStateChanged = true;
            }
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            playModeState = state;
        }

        private void CheckToUpdateMeshesImmediately()
        {
            if (!updateImmediately)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                timeWhenUnityLastUpdated = Time.realtimeSinceStartup;

            if (timeSinceUnityUpdated < 1) //likely recompiling
                return;

            if (chunk.IsAutomaticTerrainRecreationDisabled || hasChunkConnected)
                return;
            
            bool justSelected = previousSelection != gameObject && Selection.activeGameObject == gameObject;
            if (justSelected)
                return;

            bool justDeselected = previousSelection == gameObject && Selection.activeGameObject != gameObject;
            if (justDeselected)
                return;

            bool isSelected = Selection.activeGameObject == gameObject;
            if (!isSelected)
                return;
            
            if (playModeState is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.ExitingPlayMode
                || (Application.isPlaying && !GameLoaderSceneManager.HasLoaded))
                return;

            if (chunk.CurrentTerrain == null)
                return;
            
            EditorApplication.delayCall -= RecreateTerrain;
            EditorApplication.delayCall += RecreateTerrain;
        }
        
        /// <summary>
        /// Force the terrain to be recreated.
        /// </summary>
        public void RecreateTerrain()
        {
            DisconnectAll();
            
            GlobalLoggers.ChunkLogger.Log($"Recreating terrain for '{chunk.name}'");
            Material[] previousMaterials = chunk.CurrentTerrain.GetComponent<MeshRenderer>().sharedMaterials;
            DestroyImmediate(chunk.CurrentTerrain);

            chunk.SplineComputer.RebuildImmediate();
            
            GameObject newTerrain = terrainData.Create(chunk, previousMaterials);
            chunk.SetTerrain(newTerrain);
            
            if (chunkBefore != null)
                ChunkUtils.ConnectChunks(chunkBefore, chunk, ChunkUtils.LoadDirection.AFTER, new ChunkBlendData(chunkBefore, chunk));
            if (chunkAfter != null)
                ChunkUtils.ConnectChunks(chunk, chunkAfter, ChunkUtils.LoadDirection.AFTER, new ChunkBlendData(chunk, chunkAfter));

            UnbakeSplineMeshes();
        }

        private void UnbakeSplineMeshes()
        {
            foreach (SplineMesh splineMesh in chunk.SplinesMeshes)
            {
                if (!splineMesh.gameObject.activeSelf)
                    continue;
                splineMesh.Unbake();
            }
        }
        
        #endregion
        
        #region Connect to a chunk
        [Header("Connect")]
        [SerializeField] private Chunk chunkToConnectWith;

        public void OnConnectChunkBefore(Chunk chunk)
        {
            OnConnectChunk();
            chunkBefore = chunk;
        }
        
        public void OnConnectChunkAfter(Chunk chunk)
        {
            OnConnectChunk();
            chunkAfter = chunk;
        }

        private void OnConnectChunk()
        {
            
        }

        private void OnDisconnectChunk()
        {
            if (!hasChunkConnected)
                transform.rotation = Quaternion.Euler(Vector3.zero); //reset rotation
        }
        
        
        public void DisconnectAll(bool canUndo = false)
        {
            if (canUndo)
            {
                List<Object> objectsToRecord = new List<Object>();

                objectsToRecord.Add(transform);
                objectsToRecord.Add(chunk.CurrentTerrain.GetComponent<MeshFilter>());

                if (chunkAfter != null)
                {
                    objectsToRecord.Add(chunkAfter);
                    if (chunkAfter.GetComponent<ChunkEditorTools>().chunkBefore != null)
                        objectsToRecord.Add(chunkAfter.GetComponent<ChunkEditorTools>().chunkBefore);
                }

                if (chunkBefore != null)
                {
                    objectsToRecord.Add(chunkBefore);
                    if (chunkAfter.GetComponent<ChunkEditorTools>().chunkAfter != null)
                        objectsToRecord.Add(chunkAfter.GetComponent<ChunkEditorTools>().chunkAfter);
                }
                
                Undo.RecordObjects(objectsToRecord.ToArray(), "Disconnect Chunk");
            }

            OnDisconnectChunkAfter();
            OnDisconnectChunkBefore();
        }

        public void OnDisconnectChunkBefore()
        {
            if (chunkBefore == null)
                return;

            Chunk previousChunk = chunkBefore;
            chunkBefore = null;
            previousChunk.GetComponent<ChunkEditorTools>().OnDisconnectChunkAfter();
            OnDisconnectChunk();
        }

        public void OnDisconnectChunkAfter()
        {
            if (chunkAfter == null)
                return;
            
            Chunk previousChunk = chunkAfter;
            chunkAfter = null;
            previousChunk.GetComponent<ChunkEditorTools>().OnDisconnectChunkBefore();
            OnDisconnectChunk();
        }

        /// <summary>
        /// Connect the chunk with the specified chunk.
        /// </summary>
        [ButtonMethod]
        public void Connect()
        {
            if (chunkToConnectWith == null)
                throw new NullReferenceException($"There is no '{nameof(chunkToConnectWith)}' value set in the inspector.");
            
            if (hasChunkConnected)
                throw new InvalidOperationException("This chunk is already connected. Disconnect the chunk first.");

            ChunkUtils.ConnectChunksWithNewBlendData(chunkToConnectWith, chunk, ChunkUtils.LoadDirection.AFTER, true);
        }
        
        [ButtonMethod]
        public void Disconnect()
        {
            if (!hasChunkConnected)
            {
                Debug.Log("This chunk is already disconnected.");
                return;
            }

            Chunk[] affectedChunks = { chunk, chunkAfter, chunkBefore};
            DisconnectAll(true);
            foreach (Chunk affectedChunk in affectedChunks)
            {
                if (affectedChunk != null)
                    affectedChunk.GetComponent<ChunkEditorTools>().RecreateTerrain();
            }
        }

        private void CheckIfJustDeselected()
        {
            bool justDeselected = previousSelection == gameObject && Selection.activeGameObject != gameObject;
            if (justDeselected)
                Tools.hidden = false;
        }

        private void CheckToDisableTools()
        {
            if (Selection.activeGameObject != gameObject)
                return;
            
            Tools.hidden = hasChunkConnected;
        }
        
        #endregion
#endif
    }
}