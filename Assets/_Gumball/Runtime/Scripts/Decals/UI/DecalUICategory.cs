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

    [Serializable]
    public class DecalTexture
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private bool canColour;

        public Sprite Sprite => sprite;
        public bool CanColour => canColour;
    }

}
