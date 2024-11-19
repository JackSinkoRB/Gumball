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

        [SerializeField, ReadOnly] private string guid;

        public string GUID => guid.IsNullOrEmpty() || guid.Equals("INVALID") ? null : guid;
        public WarehouseCarData CarData { get; private set; }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            FindGUIDFromCarReference();
#endif
        }

        public void OnAfterDeserialize()
        {
            
        }
        
#if UNITY_EDITOR
        private void FindGUIDFromCarReference()
        {
            guid = "INVALID";

            if (car == null || car.editorAsset == null)
                return;
            
            foreach (WarehouseCarData carData in WarehouseManager.Instance.AllCarData)
            {
                if (car.editorAsset.gameObject == carData.CarPrefabReference.editorAsset.gameObject)
                {
                    guid = carData.GUID;
                    CarData = carData;
                    break;
                }
            }
        }
#endif
        
    }
}
