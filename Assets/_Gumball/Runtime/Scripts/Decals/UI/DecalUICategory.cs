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
        [SerializeField] private Sprite[] sprites;

        public string CategoryName => categoryName;
        public Sprite[] Sprites => sprites;
    }
}
