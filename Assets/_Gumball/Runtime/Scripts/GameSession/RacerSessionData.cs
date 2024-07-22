using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Gumball
{
    [Serializable]
    public class RacerSessionData
    {
        
        [SerializeField] private AssetReferenceGameObject assetReference;
        [Tooltip("Can the racer cross the middle of the chunks? Disabling this will enable an invisible barrier for the car in the middle.")]
        [SerializeField] private bool canCrossMiddle = true;
        [Tooltip("This is the maximum distance +/- from the racing line that the racer could drive (randomly chosen).")]
        [SerializeField] private float racingLineImprecisionMaxDistance = 3f;
        [SerializeField] private PositionAndRotation startingPosition;
        [SerializeField] private CarPerformanceProfile performanceProfile;

        public AssetReferenceGameObject AssetReference => assetReference;
        public PositionAndRotation StartingPosition => startingPosition;
        public bool CanCrossMiddle => canCrossMiddle;
        public CarPerformanceProfile PerformanceProfile => performanceProfile;
        
        public float GetRandomRacingLineImprecision()
        {
            return Random.Range(-racingLineImprecisionMaxDistance, racingLineImprecisionMaxDistance);
        }
        
    }
}
