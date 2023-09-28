using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Map Data")]
    public class MapData : ScriptableObject
    {

        [SerializeField] private int startingChunkIndex;
        [SerializeField] private Vector3 vehicleStartingPosition;
        [SerializeField] private Vector3 vehicleStartingRotation;
        
        [Space(5)]
        [SerializeField] private AssetReferenceGameObject[] chunks;

        public int StartingChunkIndex => startingChunkIndex;
        public Vector3 VehicleStartingPosition => vehicleStartingPosition;
        public Vector3 VehicleStartingRotation => vehicleStartingRotation;
        public AssetReferenceGameObject[] ChunkReferences => chunks;
        
    }
}
