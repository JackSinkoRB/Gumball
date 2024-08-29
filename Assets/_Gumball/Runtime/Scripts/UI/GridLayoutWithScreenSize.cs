using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [ExecuteAlways]
    public class GridLayoutWithScreenSize : MonoBehaviour
    {

        private enum LayoutDirection
        {
            HORIZONTAL,
            VERTICAL
        }

        [SerializeField] private LayoutDirection layoutDirection;
        [SerializeField, ConditionalField(nameof(layoutDirection), true)] private int numberOfColumns = 1;
        [SerializeField, ConditionalField(nameof(layoutDirection))] private int numberOfRows = 1;

        private void OnValidate()
        {
            Resize();
        }

        [ButtonMethod]
        public void Resize()
        {
            
        }
        
    }
}
