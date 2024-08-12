using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Event")]
    public class GumballEvent : ScriptableObject
    {

        [SerializeField] private AssetReferenceGameObject[] maps;

        public AssetReferenceGameObject[] Maps => maps;

#if UNITY_EDITOR
        [ButtonMethod]
        public void RebuildAllMaps()
        {
            if (Application.isPlaying)
                throw new InvalidOperationException("Cannot rebuild during play mode.");

            HashSet<AssetReferenceT<ChunkMap>> chunkMapsToRebuild = new();

            //find all the chunks maps
            foreach (AssetReferenceGameObject mapReference in maps)
            {
                GameSessionMap map = mapReference.editorAsset.GetComponent<GameSessionMap>();
                foreach (GameSessionNode node in map.Nodes)
                {
                    AssetReferenceT<ChunkMap> chunkMapReference = node.GameSession.ChunkMapAssetReference;
                    chunkMapsToRebuild.Add(chunkMapReference);
                }
            }

            //rebuild the data (but only recreate runtime chunks once)
            foreach (AssetReferenceT<ChunkMap> chunkMap in chunkMapsToRebuild)
            {
                chunkMap.editorAsset.RebuildData(false);
            }
            
            //reset the runtime chunk creation tracking
            ChunkMap.ClearRuntimeChunksCreatedTracking();
        }
#endif
        
    }
}
