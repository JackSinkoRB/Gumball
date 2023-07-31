using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
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
            
            ChunkManager.ConnectChunks(chunkToConnectWith, chunk, true);
        }
        #endregion
        
    }
}
