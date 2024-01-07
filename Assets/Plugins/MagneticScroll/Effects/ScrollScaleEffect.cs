using UnityEngine;

namespace MagneticScrollUtils
{
    public class ScrollScaleEffect : ScrollEffect
    {
        public enum ScaleAxis
        {
            Both,
            X,
            Y,
        }

        [SerializeField] private ScaleAxis scaleAxis;
        [SerializeField] private AnimationCurve scaleEffectCurve;

        public override void ApplyEffectToIcon(MagneticScroll.Direction direction, ScrollIcon icon, float positionPercentage)
        {
            float scaleValue = scaleEffectCurve.Evaluate(positionPercentage);
            icon.SetScaleInRespectToDefaultScaleForAxis(scaleAxis, scaleValue);
        }
    }
}