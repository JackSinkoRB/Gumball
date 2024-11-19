using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Serializable]
    public class CarDataReference : ISerializationCallbackReceiver
    {

#if UNITY_EDITOR
        [SerializeField] private AssetReferenceGameObject car;
#endif

        [SerializeField, ReadOnly] private string runtimeKey;

        public string GUID => runtimeKey.IsNullOrEmpty() || runtimeKey.Equals("INVALID") ? null : runtimeKey;
        public WarehouseCarData CarData { get; private set; }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (car == null || !car.RuntimeKeyIsValid() || !car.RuntimeKey.ToString().Equals(runtimeKey))
                FindGUIDFromCarReference();
#endif
        }

        public void OnAfterDeserialize()
        {
            
        }
        
#if UNITY_EDITOR
        private void FindGUIDFromCarReference()
        {
            runtimeKey = "INVALID";

            if (car == null || !car.RuntimeKeyIsValid())
                return;
            
            foreach (WarehouseCarData carData in WarehouseManager.Instance.AllCarData)
            {
                if (car.editorAsset.gameObject == carData.CarPrefabReference.editorAsset.gameObject)
                {
                    CarData = carData;
                    runtimeKey = car.RuntimeKey.ToString();
                    break;
                }
            }
        }
#endif
        
    }
}
