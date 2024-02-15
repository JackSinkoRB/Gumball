using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Event")]
    public class GumballEvent : ScriptableObject
    {

        [SerializeField] private AssetReferenceGameObject[] maps;

        public AssetReferenceGameObject[] Maps => maps;

    }
}
