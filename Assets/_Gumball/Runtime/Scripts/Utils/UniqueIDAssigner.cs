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
    public delegate void OnAssignIDDelegate(UniqueIDAssigner uniqueIDAssigner, string previousID, string newID);
    public static event OnAssignIDDelegate OnAssignID;
    
    private void Update()
    {
        if (Application.isPlaying)
            return;
            
        TryGenerateNewID();
    }

    private void TryGenerateNewID()
    {
        string sceneName = GetSceneName();
        
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
            GenerateNewID();
        }

        if (!allIDs.ContainsKey(uniqueID) || allIDs[uniqueID] == null)
            allIDs[uniqueID] = this;
    }
    
    private void GenerateNewID()
    {
        string sceneName = GetSceneName();
        string prefix = $"{sceneName}_";
        string previousId = uniqueID;
        uniqueID = prefix + Guid.NewGuid();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log($"Generating new ID for {gameObject.name}");

        OnAssignID?.Invoke(this, previousId, uniqueID);
    }
    
    private string GetSceneName()
    {
        string sceneName = gameObject.scene.name;

        bool isPrefab = sceneName == null || sceneName.Equals(gameObject.name);
        if (isPrefab)
            sceneName = "Prefab";

        return sceneName;
    }
    
    private class DetectDuplicates : AssetModificationProcessor
    {
        public static readonly List<string> newAssets = new();
        
        private static void OnWillCreateAsset(string assetPath)
        {
            newAssets.Add(assetPath);
        }
    }

    private class DetectDuplicatesPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (DetectDuplicates.newAssets.Count == 0)
                return;
            
            foreach (string str in importedAssets)
            {
                if (!DetectDuplicates.newAssets.Contains(str))
                    continue;
                
                GameObject obj = AssetDatabase.LoadAssetAtPath(str,typeof(GameObject)) as GameObject;
                if (obj == null)
                    continue;
                
                UniqueIDAssigner uniqueIDAssigner = obj.GetComponent<UniqueIDAssigner>();
                if (uniqueIDAssigner == null)
                    continue;
                
                uniqueIDAssigner.GenerateNewID();
            }
            
            DetectDuplicates.newAssets.Clear();
        }
    }
#endif

}