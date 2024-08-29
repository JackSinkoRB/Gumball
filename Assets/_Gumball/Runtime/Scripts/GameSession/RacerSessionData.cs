using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Gumball
{
    [Serializable]
    public class RacerSessionData
    {
        
        [SerializeField] private AssetReferenceGameObject assetReference;
        [SerializeField] private PositionAndRotation startingPosition;

        [Header("Behaviour")]
        [Tooltip("Can the racer cross the middle of the chunks? Disabling this will enable an invisible barrier for the car in the middle.")]
        [SerializeField] private bool canCrossMiddle = true;
        [Tooltip("This is the maximum distance +/- from the racing line that the racer could drive (randomly chosen).")]
        [SerializeField] private float racingLineImprecisionMaxDistance = 3f;
        
        [Header("Performance")]
        [SerializeField] private CarPerformanceProfile performanceProfile;
        [SerializeField, ReadOnly] private PerformanceRatingCalculator currentPerformanceRating;

        [Header("Customisation")]
        [SerializeField] private ColourSwatch bodyPaintSwatch = new();
        [SerializeField] private ColourSwatch wheelPaintSwatch = new();
        
        public AssetReferenceGameObject AssetReference => assetReference;
        public PositionAndRotation StartingPosition => startingPosition;
        public bool CanCrossMiddle => canCrossMiddle;
        
        public float GetRandomRacingLineImprecision()
        {
            return Random.Range(-racingLineImprecisionMaxDistance, racingLineImprecisionMaxDistance);
        }

        public void LoadIntoCar(AICar car)
        {
            car.SetPerformanceProfile(performanceProfile);

            ApplyBodyPaintToCar(car);
            ApplyWheelPaintToCar(car);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (assetReference != null && assetReference.editorAsset != null && assetReference.editorAsset.GetComponent<AICar>() != null)
                currentPerformanceRating.Calculate(assetReference.editorAsset.GetComponent<AICar>().PerformanceSettings, performanceProfile);
        }
#endif
        
        private void ApplyBodyPaintToCar(AICar carInstance)
        {
            if (carInstance.BodyPaintModification == null)
            {
                Debug.LogWarning($"Cannot colour body paint on {carInstance.name} because it is missing the {nameof(BodyPaintModification)} component.");
                return;
            }
            
            carInstance.BodyPaintModification.ApplySwatch(bodyPaintSwatch);
        }

        private void ApplyWheelPaintToCar(AICar carInstance)
        {
            foreach (WheelMesh wheelMesh in carInstance.AllWheelMeshes)
            {
                WheelPaintModification wheelPaintModification = wheelMesh.GetComponent<WheelPaintModification>();
                if (wheelPaintModification == null)
                {
                    Debug.LogWarning($"Cannot colour wheel mesh on {carInstance.name} because it is missing the {nameof(WheelPaintModification)} component.");
                    continue;
                }
                
                wheelPaintModification.ApplySwatch(wheelPaintSwatch);
            }
        }

    }
}
