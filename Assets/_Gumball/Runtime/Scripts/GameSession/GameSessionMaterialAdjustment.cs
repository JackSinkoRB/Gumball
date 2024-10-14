using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class GameSessionMaterialAdjustment
    {

        [SerializeField] private Material material;
        [SerializeField] private string propertyName;
        [SerializeField] private float value;

        public void UpdateMaterial()
        {
            material.SetFloat(propertyName, value);
        }

    }
}
