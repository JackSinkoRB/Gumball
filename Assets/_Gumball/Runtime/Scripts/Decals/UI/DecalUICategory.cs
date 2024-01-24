using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class DecalUICategory
    {
        [SerializeField] private string categoryName;
        [SerializeField] private DecalTexture[] decalTextures;

        public string CategoryName => categoryName;
        public DecalTexture[] DecalTextures => decalTextures;
    }
}
