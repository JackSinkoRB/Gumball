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
                {
                    EditorGUILayout.HelpBox("The object is not a valid prefab asset (ending in .prefab). ChunkObject can only be added to prefabs. Therefore this object will not show at runtime.", MessageType.Error);
                    if (chunkObject.enabled)
                    {
                        chunkObject.enabled = false;
                        EditorUtility.SetDirty(chunkObject);
                        EditorUtility.SetDirty(chunkObject.Chunk.gameObject);
                    }
                }

                if (chunkObject.IsChildOfAnotherChunkObject)
                {
                    EditorGUILayout.HelpBox("This ChunkObject will not function properly because it is a child of another ChunkObject.", MessageType.Error);
                    if (chunkObject.enabled)
                    {
                        chunkObject.enabled = false;
                        EditorUtility.SetDirty(chunkObject);
                        EditorUtility.SetDirty(chunkObject.Chunk.gameObject);
                    }
                }

                if (chunkObject.CanFlattenTerrain && chunkObject.ColliderToFlattenTo is MeshCollider meshCollider && !meshCollider.convex)
                    EditorGUILayout.HelpBox($"Using a non-convex MeshCollider for '{nameof(chunkObject.ColliderToFlattenTo)}' can cause the terrain generation to take a long time.\nMake sure to minimise the number of vertices or switch the collider to convex or a primitive type (eg. BoxCollider).", MessageType.Error);
            }

            base.OnInspectorGUI();
        }
        
    }
}
#endif