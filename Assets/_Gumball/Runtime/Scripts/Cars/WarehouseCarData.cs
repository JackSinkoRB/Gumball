using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Serializable]
    public class WarehouseCarData
    {
            
        [SerializeField] private AssetReferenceGameObject carPrefabReference;
        [SerializeField] private Sprite icon;
        [SerializeField, ReadOnly] private string displayName;
            
        public AssetReferenceGameObject CarPrefabReference => carPrefabReference;
        public Sprite Icon => icon;
        public string DisplayName => displayName;

        public void OnValidate()
        {
#if UNITY_EDITOR
            if (carPrefabReference.editorAsset != null)
                displayName = carPrefabReference.editorAsset.GetComponent<AICar>().DisplayName;
#endif
        }
            
    }
}
