using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Gumball
{
    [Serializable]
    public class RacerSessionData
    {

        [Header("Info")]
        [FormerlySerializedAs("assetReference")] [SerializeField] private AssetReferenceGameObject carAssetReference;
        [SerializeField] private RacerInfoProfile infoProfile;
        
        [Header("Behaviour")]
        [SerializeField] private PositionAndRotation startingPosition;
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

        public RacerInfoProfile InfoProfile => infoProfile;
        public AssetReferenceGameObject CarAssetReference => carAssetReference;
        public PositionAndRotation StartingPosition => startingPosition;
        public bool CanCrossMiddle => canCrossMiddle;
        
        public float GetRandomRacingLineImprecision()
        {
            return Random.Range(-racingLineImprecisionMaxDistance, racingLineImprecisionMaxDistance);
        }

        public void LoadIntoCar(AICar car)
        {
            car.SetPerformanceProfile(performanceProfile);

            if (infoProfile.Icon != null)
                car.RacerIcon.SetIcon(infoProfile.Icon);
            else Debug.LogError($"{car.name} doesn't have a racer icon.");
            
            ApplyBodyPaintToCar(car);
            ApplyWheelPaintToCar(car);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (carAssetReference != null && carAssetReference.editorAsset != null && carAssetReference.editorAsset.GetComponent<AICar>() != null)
                currentPerformanceRating.Calculate(carAssetReference.editorAsset.GetComponent<AICar>().PerformanceSettings, performanceProfile);
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
