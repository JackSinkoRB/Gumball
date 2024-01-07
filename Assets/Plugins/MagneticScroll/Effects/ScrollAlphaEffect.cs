using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace MagneticScrollUtils
{
    public class ScrollAlphaEffect : ScrollEffect
    {
        [SerializeField] private AnimationCurve alphaCurve;

        public override void ApplyEffectToIcon(MagneticScroll.Direction direction, ScrollIcon icon, float positionPercentage)
        {
            float alpha = alphaCurve.Evaluate(positionPercentage);
            icon.GetOrAddComponent<CanvasGroup>().alpha = alpha;
        }

    }
}