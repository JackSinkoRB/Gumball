using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Serializable]
    public class AddressableSceneReference : ISerializationCallbackReceiver
    {
        
#if UNITY_EDITOR
        [SerializeField] private AssetReferenceT<SceneAsset> scene;
#endif

        [SerializeField, ReadOnly] private string sceneName;
        [SerializeField, ReadOnly] private string address;
        
        public string SceneName => sceneName;
        public string Address => address;

#if UNITY_EDITOR
        public SceneAsset EditorAsset => scene.editorAsset;
        
        public bool IsDirty { get; private set; }
        
        public void SetDirty(bool isDirty)
        {
            IsDirty = isDirty;
        }
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            try
            {
                if (scene == null || scene.editorAsset == null)
                    sceneName = null;
                else
                {
                    string desiredName = scene.editorAsset.name;
                    if (sceneName == null || !sceneName.Equals(desiredName))
                    {
                        sceneName = desiredName;
                        SetDirty(true);
                    }
                }

                if (scene == null || scene.editorAsset == null)
                    address = null;
                else
                {
                    string desiredAddress = scene.RuntimeKey.ToString();
                    if (address == null || !address.Equals(desiredAddress))
                    {
                        address = desiredAddress;
                        SetDirty(true);
                    }
                }
            }
            catch (UnityException)
            {
                //it will throw an error if trying to fetch the editorAsset when the domain is reloading - can safely ignore
            }
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AddressableSceneReference))]
    public class AddressableSceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            float halfWidth = position.width * 0.5f;

            Rect sceneRect = new Rect(position.x, position.y, halfWidth, position.height);
            SerializedProperty sceneProperty = property.FindPropertyRelative("scene");
            EditorGUI.PropertyField(sceneRect, sceneProperty, GUIContent.none);

            Rect sceneNameRect = new Rect(position.x + halfWidth, position.y, halfWidth, position.height);
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");
            EditorGUI.PropertyField(sceneNameRect, sceneNameProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}