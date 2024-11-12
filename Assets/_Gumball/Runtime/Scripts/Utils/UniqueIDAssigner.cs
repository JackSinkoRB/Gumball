using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MyBox;
using UnityEngine;
using Object = UnityEngine.Object;

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
    
    public static UniqueIDAssigner FindAssignerWithIDInDirectory(string oldID, string directory)
    {
        //loop all the assets in the folder and load them, and check if the ID matches
        string[] filesAtPath = Directory.GetFiles(directory, "*.prefab", SearchOption.TopDirectoryOnly);

        foreach (string objectAtPath in filesAtPath)
        {
            GameObject gameObject = AssetDatabase.LoadAssetAtPath(objectAtPath, typeof(GameObject)) as GameObject;
            if (gameObject == null)
                continue;
                        
            UniqueIDAssigner idAssigner = gameObject.GetComponent<UniqueIDAssigner>();
            if (idAssigner != null && idAssigner.uniqueID.Equals(oldID))
                return idAssigner;
        }

        return null;
    }
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
            if (uniqueID.IsNullOrEmpty())
                throw new NullReferenceException($"Object has not been generated an ID: {gameObject.name}");
            return uniqueID;
        }
    }

    public void SetPersistent(bool isPersistent)
    {
        this.isPersistent = isPersistent;
    }

#if UNITY_EDITOR
    public delegate void OnAssignNewIDDelegate(UniqueIDAssigner uniqueIDAssigner, string oldID, string newID);
    public static event OnAssignNewIDDelegate onAssignNewID;
    
    public void Initialise()
    {
        if (uniqueID.IsNullOrEmpty())
            GenerateNewID();
    }
    
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

        if (uniqueID != null && (!allIDs.ContainsKey(uniqueID) || allIDs[uniqueID] == null))
            allIDs[uniqueID] = this;
    }
    
    [ButtonMethod]
    public void GenerateNewID()
    {
        string sceneName = GetSceneName();
        string scenePrefix = perSceneUniqueness ? $"{sceneName}_" : "";
        
        string oldID = uniqueID;
        uniqueID = scenePrefix + Guid.NewGuid();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log($"Generating new ID for {gameObject.name}");

        onAssignNewID?.Invoke(this, oldID, uniqueID);
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
            
            foreach (string path in importedAssets)
            {
                if (!DetectDuplicates.newAssets.Contains(path))
                    continue;
                
                GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (obj == null)
                    continue;
                
                UniqueIDAssigner uniqueIDAssigner = obj.GetComponent<UniqueIDAssigner>();
                if (uniqueIDAssigner == null)
                    continue;
                
                if (path.Contains(ChunkUtils.RuntimeChunkSuffix))
                    continue;

                if (uniqueIDAssigner.isPersistent)
                    continue;

                string oldID = uniqueIDAssigner.uniqueID;
                uniqueIDAssigner.GenerateNewID();

                //check if duplicated a chunk
                ChunkEditorTools chunk = obj.GetComponent<ChunkEditorTools>();
                if (chunk != null)
                    ChunkEditorTools.OnDuplicateChunkAsset(oldID, path, chunk);
            }
            
            DetectDuplicates.newAssets.Clear();
        }
    }
#endif
    
}