using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

#if UNITY_EDITOR
using Gumball;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class UniqueIDAssigner : MonoBehaviour
{
    
#if UNITY_EDITOR
    private static readonly Dictionary<string, UniqueIDAssigner> allIDs = new();
#endif

    [Tooltip("Whether the object is unique per scene, or whether it is just unique on a global scale.")]
    [SerializeField] private bool perSceneUniqueness;
    [ReadOnly, SerializeField] private string uniqueID;

    [Tooltip("If enabled, the uniqueID will not be able to change.")]
    [SerializeField, ReadOnly] private bool isPersistent;
    
    public string UniqueID
    {
        get
        {
            if (uniqueID == null)
                throw new NullReferenceException($"Object has not been generated an ID: {gameObject.name}");
            return uniqueID;
        }
    }

    public void SetPersistent(bool isPersistent)
    {
        this.isPersistent = isPersistent;
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
        if (isPersistent)
            return;
        
        string sceneName = GetSceneName();

        bool hasSceneNameAtBeginning = uniqueID != null && 
                                       uniqueID.Length > sceneName.Length && 
                                       uniqueID.Substring(0, sceneName.Length).Equals(sceneName);

        bool needToGenerateNewID = perSceneUniqueness && !hasSceneNameAtBeginning;
        if (needToGenerateNewID)
        {
            GenerateNewID();
        }

        if (!allIDs.ContainsKey(uniqueID) || allIDs[uniqueID] == null)
            allIDs[uniqueID] = this;
    }
    
    [ButtonMethod]
    public void GenerateNewID()
    {
        string sceneName = GetSceneName();
        string scenePrefix = perSceneUniqueness ? $"{sceneName}_" : "";
        
        string previousId = uniqueID;
        uniqueID = scenePrefix + Guid.NewGuid();
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
                
                if (str.Contains(ChunkUtils.RuntimeChunkSuffix))
                    continue;

                if (uniqueIDAssigner.isPersistent)
                    continue;

                uniqueIDAssigner.GenerateNewID();
            }
            
            DetectDuplicates.newAssets.Clear();
        }
    }
#endif

}