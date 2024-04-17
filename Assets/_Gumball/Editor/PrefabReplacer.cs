using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class PrefabReplacer : EditorWindow
{
    GameObject _prefab;
    GameObject _targetObject;

    void OnGUI()
    {
        _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
        _targetObject = (GameObject)EditorGUILayout.ObjectField("Replace all children", _targetObject, typeof(GameObject), true);
        if (_prefab != null && GUILayout.Button("Replace Selected"))
        {
            List<Object> newobjs = new List<Object>();
            GameObject[] selected = Selection.gameObjects;
            foreach (GameObject obj in selected)
            {
                GameObject newobj = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, obj.scene);
                newobj.name = _prefab.name;
                newobj.transform.parent = obj.transform.parent;
                newobj.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());
                newobj.transform.localPosition = obj.transform.localPosition;
                newobj.transform.localRotation = obj.transform.localRotation;
                newobj.transform.localScale = obj.transform.localScale;
                newobjs.Add(newobj);
                Undo.RegisterCreatedObjectUndo(newobj, "New Undo");
                Undo.DestroyObjectImmediate(obj);
            }
            Selection.objects = newobjs.ToArray();
        }

        if(_targetObject!=null || Selection.activeGameObject!=null && Selection.activeGameObject.transform.childCount>0)
        {
            if (GUILayout.Button("Apply To All Children"))
            {
                Undo.RecordObject(null, "Base Undo");
                int undoID = Undo.GetCurrentGroup();

                //Undo.RecordObject(Selection.activeGameObject, "Modify Prefabs");
                List<Object> newobjs = new List<Object>();
                List<GameObject> selectedChildren = new List<GameObject>();
                if(_targetObject!=null)
                {
                    foreach (Transform obj in _targetObject.GetComponentsInChildren<Transform>())
                    {

                        selectedChildren.Add(obj.gameObject);
                    }
                }
                else
                {
                    foreach (Transform obj in Selection.activeGameObject.GetComponentsInChildren<Transform>())
                    {

                        selectedChildren.Add(obj.gameObject);
                    }
                }
                
                selectedChildren.Remove(selectedChildren[0]);//remove the selected object
                GameObject[] selected = selectedChildren.ToArray();
                foreach (GameObject obj in selected)
                {
                    GameObject newobj = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, obj.scene);
                    newobj.name = _prefab.name;
                    newobj.transform.parent = obj.transform.parent;
                    newobj.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());
                    newobj.transform.localPosition = obj.transform.localPosition;
                    newobj.transform.localRotation = obj.transform.localRotation;
                    newobj.transform.localScale = obj.transform.localScale;
                    newobjs.Add(newobj);
                    Undo.DestroyObjectImmediate(obj);
                    Undo.RegisterCreatedObjectUndo(newobj, "New Undo");
                    //Undo.RegisterCreatedObjectUndo(obj, "New Undo");
                    Undo.CollapseUndoOperations(undoID);
                }
                //Selection.objects = newobjs.ToArray();
            }
        }
        
    }

    [MenuItem("Custom/Prefab Replacer", false, 'P')]
    static void Init()
    {
        GetWindow(typeof(PrefabReplacer)).Show();
    }
}
#endif