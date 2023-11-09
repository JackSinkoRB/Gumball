using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ScrollAlphaEffect : ScrollEffect
    {
        [SerializeField] private AnimationCurve alphaCurve;

        public override void ApplyEffectToIcon(MagneticScroll.Direction direction, ScrollIcon icon, float positionPercentage)
        {
            float alpha = alphaCurve.Evaluate(positionPercentage);
            icon.GetComponent<CanvasGroup>(true).alpha = alpha;
        }

    }
}
