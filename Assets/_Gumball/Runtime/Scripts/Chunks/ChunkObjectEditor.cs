#if UNITY_EDITOR
using MyBox.Internal;
using UnityEngine;
using UnityEditor;

namespace Gumball.Editor
{
    [CustomEditor(typeof(ChunkObject))]
    public class ChunkObjectEditor : UnityObjectEditor
    {

        private ChunkObject chunkObject => target as ChunkObject;

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                bool isPrefab = GameObjectUtils.GetPathToPrefabAsset(chunkObject.gameObject) != null;
                if (!isPrefab)
                    EditorGUILayout.HelpBox("The object is not a valid prefab asset (ending in .prefab). ChunkObject can only be added to prefabs. Therefore this object will not show at runtime.", MessageType.Error);

                if (chunkObject.IsChildOfAnotherChunkObject)
                    EditorGUILayout.HelpBox("This ChunkObject will not function properly because it is a child of another ChunkObject.", MessageType.Error);
            }

            base.OnInspectorGUI();
        }
        
    }
}
#endif