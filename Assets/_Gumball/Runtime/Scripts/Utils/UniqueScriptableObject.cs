using System.Collections;
using System.Collections.Generic;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    public class UniqueScriptableObject : ScriptableObject
    {

        [SerializeField, ReadOnly] private string uniqueID;
        [SerializeField, HideInInspector] private string lastKnownName;

        public string ID => uniqueID;
        
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(lastKnownName)
                || !lastKnownName.Equals(name)
                || string.IsNullOrEmpty(uniqueID))
            {
                GenerateNewID();
            }
        }

        private void GenerateNewID()
        {
            lastKnownName = name;
            uniqueID = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
        
    }
}
