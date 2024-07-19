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

        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(lastKnownName)
                    || !lastKnownName.Equals(name)
                    || string.IsNullOrEmpty(uniqueID))
                {
                    GenerateNewID();
                }

                return uniqueID;
            }
        }
        
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(lastKnownName)
                || !lastKnownName.Equals(name)
                || string.IsNullOrEmpty(uniqueID))
            {
                GenerateNewID();
            }
#endif
        }

#if UNITY_EDITOR
        private void GenerateNewID()
        {
            lastKnownName = name;
            uniqueID = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
        
    }
}
