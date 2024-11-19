#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using MyBox.Internal;
using UnityEditor;
using UnityEngine;

namespace Gumball.Editor
{
    [CustomEditor(typeof(GameSessionNode), editorForChildClasses: true)]
    public class GameSessionNodeEditor : UnityObjectEditor
    {
        
        private GameSessionNode gameSessionNode => (GameSessionNode)target;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //OnInspectorGUI() doesn't get called when using DisplayInspector attribute, so call it here
            GameSessionEditor.OnInspectorGUI(gameSessionNode.GameSession);
        }

    }
}
#endif