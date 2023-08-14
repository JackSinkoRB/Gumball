using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [ExecuteAlways]
    public class ChunkEditorTools : MonoBehaviour
    {

        private Chunk chunk => GetComponent<Chunk>();
        private GameObject previousSelection;
        private float timeWhenUnityLastUpdated = 0;

        private float timeSinceUnityUpdated => Time.realtimeSinceStartup - timeWhenUnityLastUpdated;
        
        private void OnEnable()
        {
            chunk.SplineComputer.onRebuild += CheckToUpdateTerrainImmediately;
        }

        private void OnDisable()
        {
            chunk.SplineComputer.onRebuild -= CheckToUpdateTerrainImmediately;

            Tools.hidden = false;
        }

        private void OnDrawGizmos()
        {
            currentGrid?.OnDrawGizmos();
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

        #region Generate terrain

        [Header("Create terrain")]
        [ReadOnly(nameof(HasConnection)), SerializeField]
        private ChunkTerrainData terrainData = new();
        [Tooltip("If enabled, the terrain will update whenever a value is changed. Otherwise the CreateTerrain button will need to be used.")]
        [SerializeField] private bool updateImmediately = true;

        [HideInInspector] public bool HasConnection; 

        private ChunkGrid currentGrid;
        
        private static bool subscribedToPlayModeStateChanged;
        private static PlayModeStateChange playModeState;
        
        [ButtonMethod]
        public void ShowTerrainGrid()
        {
            currentGrid = new ChunkGrid(chunk, terrainData.Resolution, terrainData.WidthAroundRoad, true);
        }
        
        [ButtonMethod]
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

        private void CheckToUpdateTerrainImmediately()
        {
            if (!updateImmediately)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                timeWhenUnityLastUpdated = Time.realtimeSinceStartup;

            if (timeSinceUnityUpdated < 1) //likely recompiling
                return;

            if (chunk.IsConnecting || chunk.HasChunkConnected)
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
                || (Application.isPlaying && !LoadingSceneManager.HasLoaded))
                return;

            if (chunk.CurrentTerrain == null)
                return;
            
            EditorApplication.delayCall -= RecreateTerrain;
            EditorApplication.delayCall += RecreateTerrain;
        }
        
        /// <summary>
        /// Force the terrain to be recreated.
        /// </summary>
        private void RecreateTerrain()
        {
            Chunk connectedAfter = chunk.ChunkAfter;
            Chunk connectedBefore = chunk.ChunkBefore;
            chunk.DisconnectAll();
            
            GlobalLoggers.TerrainLogger.Log($"Recreating terrain for '{chunk.name}'");
            Material[] previousMaterials = chunk.CurrentTerrain.GetComponent<MeshRenderer>().sharedMaterials;
            DestroyImmediate(chunk.CurrentTerrain);

            chunk.SplineComputer.RebuildImmediate();
            
            GameObject newTerrain = terrainData.Create(chunk, previousMaterials);
            chunk.SetTerrain(newTerrain);
            
            if (connectedBefore != null)
                ChunkUtils.ConnectChunks(connectedBefore, chunk);
            if (connectedAfter != null)
                ChunkUtils.ConnectChunks(chunk, connectedAfter);
        }
        
        #endregion
        
        #region Connect to a chunk
        [Header("Connect")]
        [SerializeField] private Chunk chunkToConnectWith;

        /// <summary>
        /// Connect the chunk with the specified chunk.
        /// </summary>
        [ButtonMethod]
        public void Connect()
        {
            if (chunkToConnectWith == null)
                throw new NullReferenceException($"There is no '{nameof(chunkToConnectWith)}' value set in the inspector.");
            
            if (chunk.HasChunkConnected)
                throw new InvalidOperationException("This chunk is already connected. Disconnect the chunk first.");

            ChunkUtils.ConnectChunks(chunkToConnectWith, chunk, true);
        }
        
        [ButtonMethod]
        public void Disconnect()
        {
            if (!chunk.HasChunkConnected)
            {
                Debug.Log("This chunk is already disconnected.");
                return;
            }

            Chunk[] affectedChunks = { chunk, chunk.ChunkAfter, chunk.ChunkBefore};
            chunk.DisconnectAll(true);
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
            
            Tools.hidden = chunk.HasChunkConnected;
        }
        
        #endregion
        
        private void OnValidate()
        {
            CheckToUpdateTerrainImmediately();
        }
    }
}
