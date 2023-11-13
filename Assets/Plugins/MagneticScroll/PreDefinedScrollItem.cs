using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace MagneticScrollUtils
{
    [RequireComponent(typeof(ScrollIcon))]
    public class PreDefinedScrollItem : MonoBehaviour
    {

        [ReadOnly] public ScrollItem scrollItem;
        [ReadOnly] public ScrollIcon scrollIcon;

        private void OnValidate()
        {
            if (scrollIcon == null)
            {
                scrollIcon = GetComponent<ScrollIcon>(); //never null - requires component
            }
        }
    }
}