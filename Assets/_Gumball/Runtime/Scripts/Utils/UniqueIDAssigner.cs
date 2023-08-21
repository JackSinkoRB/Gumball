using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class UniqueIDAssigner : MonoBehaviour
{
    
#if UNITY_EDITOR
    private static readonly Dictionary<string, UniqueIDAssigner> allIDs = new();
#endif
    
    [ReadOnly, SerializeField] private string uniqueID;

    public string UniqueID
    {
        get
        {
            if (uniqueID == null)
                throw new NullReferenceException($"Object has not been generated an ID: {gameObject.name}");
            return uniqueID;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying)
            return;
            
        TryGenerateNewID();
    }

    private void TryGenerateNewID()
    {
        string sceneName = gameObject.scene.name;

        bool isPrefab = sceneName.Equals(gameObject.name);
        if (isPrefab)
            sceneName = "Prefab";

        //check if another object has the same ID and set null
        bool anotherComponentAlreadyHasThisID = uniqueID != null && 
                                                allIDs.ContainsKey(uniqueID) &&
                                                allIDs[uniqueID] != null &&
                                                allIDs[uniqueID] != this;

        bool hasSceneNameAtBeginning = uniqueID != null && 
                                       uniqueID.Length > sceneName.Length && 
                                       uniqueID.Substring(0, sceneName.Length).Equals(sceneName);

        bool needToGenerateNewID = !hasSceneNameAtBeginning || anotherComponentAlreadyHasThisID;
        if (needToGenerateNewID)
        {
            string prefix = $"{sceneName}_";
            uniqueID = prefix + Guid.NewGuid();
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        if (!allIDs.ContainsKey(uniqueID) || allIDs[uniqueID] == null)
            allIDs[uniqueID] = this;
    }

#endif
    
}