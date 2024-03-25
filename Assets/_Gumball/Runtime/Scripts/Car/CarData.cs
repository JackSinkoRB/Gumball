using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Obsolete]
    [Serializable]
    public class CarData
    {

        [SerializeField] private AssetReferenceGameObject assetReference;

        public AssetReferenceGameObject AssetReference => assetReference;
        
    }
}