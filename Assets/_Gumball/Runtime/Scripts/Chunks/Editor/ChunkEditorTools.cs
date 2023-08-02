using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

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
        
        [ButtonMethod]
        public void ShowTerrainGrid()
        {
            new ChunkGrid(chunk, terrainData.Resolution, terrainData.WidthAroundRoad, true);
        }
        
        [ButtonMethod]
        public void GenerateTerrain()
        {
            terrainData.GenerateTerrain(chunk);
        }
        #endregion
        
    }
}
