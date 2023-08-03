using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dreamteck.Splines;
using MyBox;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    public class ChunkEditorTools : MonoBehaviour
    {

        private Chunk chunk => GetComponent<Chunk>();

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
        [SerializeField] private ChunkTerrainData terrainData = new();
        [Tooltip("If enabled, the terrain will update whenever a value is changed. Otherwise the CreateTerrain button will need to be used.")]
        [SerializeField] private bool updateImmediately = true;
        
        private GameObject currentTerrain;

        [ButtonMethod]
        public void ShowTerrainGrid()
        {
            new ChunkGrid(chunk, terrainData.Resolution, terrainData.WidthAroundRoad, true);
        }
        
        [ButtonMethod]
        public void CreateTerrain()
        {
            currentTerrain = terrainData.Create(chunk);
            Selection.SetActiveObjectWithContext(currentTerrain, chunk);
        }

        private void CheckToUpdateTerrainImmediately()
        {
            if (!updateImmediately || Application.isPlaying)
                return;
            
            if (currentTerrain == null)
                return;
            
            EditorApplication.delayCall+=()=>
            {
                DestroyImmediate(currentTerrain);
                currentTerrain = terrainData.Create(chunk);
            };
        }
        #endregion
        
        private void OnValidate()
        {
            CheckToUpdateTerrainImmediately();
        }
    }
}
