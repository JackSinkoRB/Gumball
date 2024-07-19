#if UNITY_EDITOR
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

        private void OnEnable()
        {
            if (car != null && rigidbody != null) 
                rigidbody.hideFlags = HideFlags.NotEditable;
        }
        
        public override void OnInspectorGUI()
        {
            if (rigidbody != null)
                EditorGUILayout.HelpBox("Rigidbody component driven by car.", MessageType.Info);
            
            base.OnInspectorGUI();
        }
        
    }
}
#endif