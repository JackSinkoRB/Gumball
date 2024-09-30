#if UNITY_EDITOR
using System;
using MyBox.Internal;
using UnityEngine;
using UnityEditor;

namespace Gumball.Editor
{
    [CustomEditor(typeof(AICar))]
    public class AICarEditor : UnityObjectEditor
    {

        private AICar car => target as AICar;
        private Rigidbody rigidbody => car.GetComponent<Rigidbody>();

        public override void OnInspectorGUI()
        {
            if (rigidbody != null)
                EditorGUILayout.HelpBox("Rigidbody component driven by car.", MessageType.Info);

            if (car != null && rigidbody != null)
                rigidbody.hideFlags = car.CanBeDrivenByPlayer ? HideFlags.NotEditable : HideFlags.None;
            
            base.OnInspectorGUI();
        }
        
    }
}
#endif