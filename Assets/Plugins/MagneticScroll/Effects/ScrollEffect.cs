using UnityEngine;

namespace MagneticScrollUtils
{
    public abstract class ScrollEffect : MonoBehaviour
    {

        public abstract void ApplyEffectToIcon(MagneticScroll.Direction direction, ScrollIcon icon, float positionPercentage);

    }
}