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
        }

        private void OnDrawGizmos()
        {
            currentGrid?.OnDrawGizmos();
        }

        private void Update()
        {
            previousSelection = Selection.activeGameObject;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                timeWhenUnityLastUpdated = Time.realtimeSinceStartup;
        }
        
        #region Connect to a chunk
        [SerializeField] private Chunk chunkToConnectWith;
        
        /// <summary>
        /// Connect the chunk with the specified chunk.
        /// </summary>
        [ButtonMethod]
        public void Connect()
        {
            if (chunkToConnectWith == null)
                throw new NullReferenceException($"There is no '{nameof(chunkToConnectWith)}' value set in the inspector.");
            
            ChunkUtils.ConnectChunks(chunkToConnectWith, chunk, true);
        }
        #endregion

        #region Generate terrain
        [ReadOnly, SerializeField] private GameObject currentTerrain;
        [SerializeField] private ChunkTerrainData terrainData = new();
        [Tooltip("If enabled, the terrain will update whenever a value is changed. Otherwise the CreateTerrain button will need to be used.")]
        [SerializeField] private bool updateImmediately = true;

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
            currentTerrain = terrainData.Create(chunk);
            currentGrid = terrainData.Grid;
            
            Selection.SetActiveObjectWithContext(currentTerrain, chunk);
            Undo.RegisterCreatedObjectUndo(currentTerrain, "Create Terrain");
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

            TryFindExistingTerrain();
            if (currentTerrain == null)
                return;

            EditorApplication.delayCall+=()=>
            {
                if (currentTerrain == null
                    || playModeState == PlayModeStateChange.ExitingEditMode || playModeState == PlayModeStateChange.ExitingPlayMode)
                    return;

                GlobalLoggers.TerrainLogger.Log($"Recreating terrain for '{chunk.name}'");
                Material[] previousMaterials = currentTerrain.GetComponent<MeshRenderer>().sharedMaterials;
                DestroyImmediate(currentTerrain);
                currentTerrain = terrainData.Create(chunk, previousMaterials);
            };
        }

        private void TryFindExistingTerrain()
        {
            if (currentTerrain != null)
                return; //already exists

            foreach (Transform child in transform)
            {
                if (child.tag.Equals(ChunkUtils.TerrainTag))
                {
                    currentTerrain = child.gameObject;
                    return;
                }
            }
        }
        #endregion
        
        private void OnValidate()
        {
            CheckToUpdateTerrainImmediately();
        }
    }
}
