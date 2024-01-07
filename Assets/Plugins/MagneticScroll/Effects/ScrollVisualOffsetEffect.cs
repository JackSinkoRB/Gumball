using UnityEngine;

namespace MagneticScrollUtils
{
    public class ScrollVisualOffsetEffect : ScrollEffect
    {
        [SerializeField] private AnimationCurve visualOffsetEffectCurve;

        public override void ApplyEffectToIcon(MagneticScroll.Direction direction, ScrollIcon icon, float positionPercentage)
        {
            float offsetValue = visualOffsetEffectCurve.Evaluate(positionPercentage);

            icon.visualOffsetEffectRectTransform.anchoredPosition = direction switch
            {
                MagneticScroll.Direction.HORIZONTAL => new Vector2(offsetValue, 0),
                MagneticScroll.Direction.VERTICAL => new Vector2(0, offsetValue),
                _ => icon.visualOffsetEffectRectTransform.anchoredPosition
            };
        }
    }
}