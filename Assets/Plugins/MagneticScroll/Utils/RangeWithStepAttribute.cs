using System;
using UnityEngine;

namespace MyBox
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RangeWithStepAttribute : PropertyAttribute
    {
        public readonly int min;
        public readonly int max;
        public readonly int step;
        public readonly int startFrom;
        
        public RangeWithStepAttribute(int min, int max, int step, int startFrom = 0)
        {
            this.min = min;
            this.max = max;
            this.step = step;
            this.startFrom = startFrom;
        }
    }
}

#if UNITY_EDITOR
namespace MyBox.Internal
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(RangeWithStepAttribute))]
    public class RangeWithStepAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            var rangeAttribute = (RangeWithStepAttribute) attribute;

            int value = EditorGUI.IntSlider(position, label, property.intValue, rangeAttribute.min, rangeAttribute.max);

            value = rangeAttribute.startFrom + ((value / rangeAttribute.step) * rangeAttribute.step);
            property.intValue = value;
        }
    }
}
#endif