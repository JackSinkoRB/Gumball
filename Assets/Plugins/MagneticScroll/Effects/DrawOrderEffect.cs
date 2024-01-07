using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MagneticScrollUtils
{
    public class DrawOrderEffect : ScrollEffect
    {
        private const float CENTRE_POSITION = 0.5f;

        public override void ApplyEffectToIcon(MagneticScroll.Direction direction, ScrollIcon icon, float positionPercentage)
        {
            int magnetChildren = transform.childCount - 1; // accounting for the magnet itself
            float distanceStep = 1f / magnetChildren;
            float distanceToCentre = Mathf.Abs(CENTRE_POSITION - positionPercentage);
            icon.transform.SetSiblingIndex(magnetChildren - Mathf.RoundToInt(distanceToCentre / distanceStep));
        }
    }
}