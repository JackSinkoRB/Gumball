using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Map Data")]
    public class MapData : ScriptableObject
    {

        [SerializeField] private AssetReferenceGameObject[] chunks;

        public List<AssetReferenceGameObject> GetChunksAroundPosition(Vector3 position, float radius)
        {
            List<AssetReferenceGameObject> chunksAroundPosition = new();

            foreach (AssetReferenceGameObject chunk in chunks)
            {
                //TODO: distance check
                chunksAroundPosition.Add(chunk);
            }

            return chunksAroundPosition;
        }
        
    }
}
