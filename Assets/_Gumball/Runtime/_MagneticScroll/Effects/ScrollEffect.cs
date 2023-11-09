using UnityEngine;

namespace Gumball
{
    public abstract class ScrollEffect : MonoBehaviour
    {

        public abstract void ApplyEffectToIcon(MagneticScroll.Direction direction, ScrollIcon icon, float positionPercentage);

    }
}