using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gumball.Editor
{
    public class HelpBoxAttribute : PropertyAttribute
    {
        public readonly string text;
        public readonly MessageType type;
        public readonly bool onlyShowWhenDefaultValue;
        public readonly bool inverse;

        public HelpBoxAttribute(string text, MessageType type = MessageType.Info, bool onlyShowWhenDefaultValue = false,
            bool inverse = false)
        {
            this.text = text;
            this.type = type;
            this.onlyShowWhenDefaultValue = onlyShowWhenDefaultValue;
            this.inverse = inverse;
        }
    }

    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);

            HelpBoxAttribute helpBoxAttribute = attribute as HelpBoxAttribute;

            if (helpBoxAttribute.onlyShowWhenDefaultValue &&
                ((!helpBoxAttribute.inverse && !IsPropertyValueDefault(property)) ||
                 (helpBoxAttribute.inverse && IsPropertyValueDefault(property))))
                return;

            MessageType messageType = helpBoxAttribute.type;
            GUIContent content = new GUIContent(helpBoxAttribute.text);

            position.y += EditorGUI.GetPropertyHeight(property, true) + 2f;
            position.height = EditorGUIUtility.singleLineHeight * 2.5f;

            EditorGUI.HelpBox(position, content.text, messageType);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            HelpBoxAttribute helpBoxAttribute = attribute as HelpBoxAttribute;
            float height = EditorGUI.GetPropertyHeight(property, true);

            if (helpBoxAttribute.onlyShowWhenDefaultValue &&
                ((!helpBoxAttribute.inverse && !IsPropertyValueDefault(property)) ||
                 (helpBoxAttribute.inverse && IsPropertyValueDefault(property))))
                return height;

            return height + EditorGUIUtility.singleLineHeight * 2.5f + 4f; //add extra height
        }

        private bool IsPropertyValueDefault(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Integer => property.intValue == default,
                SerializedPropertyType.Boolean => property.boolValue == default,
                SerializedPropertyType.Float => Mathf.Approximately(property.floatValue, default),
                SerializedPropertyType.String => property.stringValue == default,
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                _ => false
            };
        }
    }
}