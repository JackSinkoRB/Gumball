#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagneticScrollUtils
{
    public static class MagneticScrollCreator
    {

        [MenuItem("GameObject/MagneticScroll/Vertical")]
        private static void CreateVerticalMagneticScroll() => CreateMagneticScroll(MagneticScroll.Direction.VERTICAL);

        [MenuItem("GameObject/MagneticScroll/Horizontal")]
        private static void CreateHorizontalMagneticScroll() => CreateMagneticScroll(MagneticScroll.Direction.HORIZONTAL);

        private static void CreateMagneticScroll(MagneticScroll.Direction direction)
        {
            GameObject gameObject = new GameObject("Magnetic Scroll (" + direction + ")");
            gameObject.transform.SetParent(Selection.activeTransform);
            MagneticScroll magneticScroll = gameObject.AddComponent<MagneticScroll>();
            magneticScroll.ScrollDirection = direction;
        }

    }
}
#endif