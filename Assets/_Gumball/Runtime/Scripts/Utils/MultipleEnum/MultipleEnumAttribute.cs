using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    public class MultipleEnumAttribute : PropertyAttribute
    {
        public MultipleEnumAttribute()
        {

        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MultipleEnumAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                //set to EVERYTHING if 'none'
                if (property.intValue == 0)
                    property.intValue = -1;
                
                property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use MultipleEnum with enum types");
            }
        }
    }
#endif
}