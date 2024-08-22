using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Serializable]
    public class WarehouseCarData
    {
    
        [Serializable]
        private struct CachedData
        {
            [SerializeField, ReadOnly] private string displayName;
            [SerializeField, ReadOnly] private CarPerformanceSettings performanceSettings;
            [SerializeField, ReadOnly] private CorePart defaultEngine;
            [SerializeField, ReadOnly] private CorePart defaultWheels;
            [SerializeField, ReadOnly] private CorePart defaultDrivetrain;

            public string DisplayName => displayName;
            public CarPerformanceSettings PerformanceSettings => performanceSettings;
            public CorePart DefaultEngine => defaultEngine;
            public CorePart DefaultWheels => defaultWheels;
            public CorePart DefaultDrivetrain => defaultDrivetrain;
            
            public void Cache(AICar car)
            {
                displayName = car.DisplayName;
                performanceSettings = car.PerformanceSettings;
                defaultEngine = car.GetDefaultPart(CorePart.PartType.ENGINE);
                defaultWheels = car.GetDefaultPart(CorePart.PartType.WHEELS);
                defaultDrivetrain = car.GetDefaultPart(CorePart.PartType.DRIVETRAIN);
            }
        }
            
        [SerializeField] private AssetReferenceGameObject carPrefabReference;
        [SerializeField] private Sprite icon;
        [Tooltip("The index of the first level (starting at index 0) for the car blueprint level upgrades.")]
        [SerializeField] private int levelToUnlock;
        [SerializeField, ReadOnly] private CachedData cachedData;

        public AssetReferenceGameObject CarPrefabReference => carPrefabReference;
        public Sprite Icon => icon;
        public int LevelToUnlock => levelToUnlock;
        public string DisplayName => cachedData.DisplayName;
        public CarPerformanceSettings PerformanceSettings => cachedData.PerformanceSettings;
        
        public CorePart GetDefaultPart(CorePart.PartType type)
        {
            return type switch
            {
                CorePart.PartType.ENGINE => cachedData.DefaultEngine,
                CorePart.PartType.WHEELS => cachedData.DefaultWheels,
                CorePart.PartType.DRIVETRAIN => cachedData.DefaultDrivetrain,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
#if UNITY_EDITOR
        public void CacheCarData()
        {
            if (carPrefabReference.editorAsset == null)
                return;

            AICar car = carPrefabReference.editorAsset.GetComponent<AICar>();
            if (car == null)
                return;
            
            cachedData.Cache(car);
        }
#endif

    }
}
