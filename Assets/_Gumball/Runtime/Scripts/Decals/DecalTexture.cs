using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class DecalTexture
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private bool canColour;

        public Sprite Sprite => sprite;
        public bool CanColour => canColour;
    }
}
