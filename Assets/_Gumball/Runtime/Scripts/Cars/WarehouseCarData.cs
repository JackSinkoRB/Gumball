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
            [SerializeField, ReadOnly] private string makeDisplayName;
            [SerializeField, ReadOnly] private CarType carType;
            [SerializeField, ReadOnly] private CarPerformanceSettings performanceSettings;
            [SerializeField, ReadOnly] private CorePart defaultEngine;
            [SerializeField, ReadOnly] private CorePart defaultWheels;
            [SerializeField, ReadOnly] private CorePart defaultDrivetrain;
            
            public string DisplayName => displayName;
            public string MakeDisplayName => makeDisplayName;
            public CarType CarType => carType;
            public CarPerformanceSettings PerformanceSettings => performanceSettings;
            public CorePart DefaultEngine => defaultEngine;
            public CorePart DefaultWheels => defaultWheels;
            public CorePart DefaultDrivetrain => defaultDrivetrain;
            
            public void Cache(AICar car)
            {
                displayName = car.DisplayName;
                makeDisplayName = car.MakeDisplayName;
                carType = car.CarType;
                performanceSettings = car.PerformanceSettings;
                defaultEngine = car.GetDefaultPart(CorePart.PartType.ENGINE);
                defaultWheels = car.GetDefaultPart(CorePart.PartType.WHEELS);
                defaultDrivetrain = car.GetDefaultPart(CorePart.PartType.DRIVETRAIN);
            }
        }
        
        [SerializeField] private AssetReferenceGameObject carPrefabReference;
        [SerializeField] private Sprite icon;
        [Tooltip("The level that the car needs to be unlocked.")]
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int maxLevel = 10;
        [Tooltip("If enabled, the car will be unlocked at the start of the game (eg. a starting car).")]
        [SerializeField] private bool isUnlockedByDefault;
        [SerializeField] private PurchaseData costToUnlock;
        [Tooltip("Copy the live decal data from a car to set it as the base livery here.")]
        [SerializeField] private LiveDecalData[] baseDecalData;
        [SerializeField, ReadOnly] private CachedData cachedData;

        [SerializeField, ReadOnly] public string GUID = Guid.NewGuid().ToString();
        
        private int carIndexCached = -1;
        public int CarIndex
        {
            get
            {
                if (carIndexCached == -1)
                    carIndexCached = WarehouseManager.Instance.AllCarData.IndexOf(this);
                return carIndexCached;
            }
        }

        public PurchaseData CostToUnlock => costToUnlock;
        public AssetReferenceGameObject CarPrefabReference => carPrefabReference;
        public Sprite Icon => icon;
        public int StartingLevelIndex => startingLevel - 1;
        public int MaxLevelIndex => maxLevel - 1;
        
        public bool IsUnlocked
        {
            get => DataManager.Warehouse.Get($"CarIsUnlocked.{CarIndex}", isUnlockedByDefault);
            set => DataManager.Warehouse.Set($"CarIsUnlocked.{CarIndex}", value);
        }

        public LiveDecalData[] BaseDecalData => baseDecalData;
        
        //cached data access:
        public string DisplayName => cachedData.DisplayName;
        public string MakeDisplayName => cachedData.MakeDisplayName;
        public CarType CarType => cachedData.CarType;
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

        public void Unlock()
        {
            IsUnlocked = true;
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
        
        public void AssignNewGUID()
        {
            GUID = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(WarehouseManager.Instance);
            Debug.Log($"Assigned new ID to {(carPrefabReference == null || carPrefabReference.editorAsset == null ? "null" : carPrefabReference.editorAsset.gameObject.name)}");
        }
#endif

    }
}
