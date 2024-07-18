using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    
#if !UNITY_EDITOR
        public enum MessageType
        {
            None,
            Info,
            Warning,
            Error
        }
#endif
    
    public class HelpBoxAttribute : PropertyAttribute
    {
        public enum Position
        {
            BELOW,
            ABOVE
        }
        
        public readonly string text;
        public readonly MessageType type;
        public readonly bool onlyShowWhenDefaultValue;
        public readonly bool inverse;
        public readonly Position position;

        public HelpBoxAttribute(string text, MessageType type = MessageType.Info, Position position = Position.BELOW, bool onlyShowWhenDefaultValue = false, bool inverse = false)
        {
            this.text = text;
            this.type = type;
            this.onlyShowWhenDefaultValue = onlyShowWhenDefaultValue;
            this.inverse = inverse;
            this.position = position;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : PropertyDrawer
    {
public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HelpBoxAttribute helpBoxAttribute = attribute as HelpBoxAttribute;
        MessageType messageType = helpBoxAttribute.type;
        GUIContent content = new GUIContent(helpBoxAttribute.text);

        float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
        float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2f;
        Rect helpBoxPosition = new Rect(position.x, position.y, position.width, helpBoxHeight);
        Rect propertyPosition = new Rect(position.x, position.y, position.width, propertyHeight);

        if (helpBoxAttribute.position == HelpBoxAttribute.Position.ABOVE)
        {
            // Adjust the positions when the HelpBox is above
            EditorGUI.HelpBox(helpBoxPosition, content.text, messageType);
            propertyPosition.y += helpBoxHeight;
            EditorGUI.PropertyField(propertyPosition, property, label, true);
        }
        else
        {
            // Adjust the positions when the HelpBox is below
            EditorGUI.PropertyField(propertyPosition, property, label, true);
            helpBoxPosition.y += propertyHeight;
            EditorGUI.HelpBox(helpBoxPosition, content.text, messageType);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HelpBoxAttribute helpBoxAttribute = attribute as HelpBoxAttribute;
        float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
        float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2f;

        if (helpBoxAttribute.onlyShowWhenDefaultValue &&
            ((!helpBoxAttribute.inverse && !IsPropertyValueDefault(property)) ||
             (helpBoxAttribute.inverse && IsPropertyValueDefault(property))))
        {
            return propertyHeight;
        }

        // Add extra height for the help box and some padding
        return propertyHeight + helpBoxHeight;
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
#endif
}
