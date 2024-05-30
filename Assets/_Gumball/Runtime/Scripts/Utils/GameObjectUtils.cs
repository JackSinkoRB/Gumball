using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using MyBox;

namespace Gumball
{
    public static class GameObjectUtils
    {

#if UNITY_EDITOR

        /// <summary>
        /// Check if the gameobject is a prefab and return the path to the prefab asset. Returns null if the prefab doesn't exist.
        /// </summary>
        public static string GetPathToPrefabAsset(GameObject gameObject)
        {
            string path;
            PrefabAssetType assetType = PrefabUtility.GetPrefabAssetType(gameObject);
            if (assetType is PrefabAssetType.Regular or PrefabAssetType.Variant)
            {
                path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                if (!path.IsNullOrEmpty() && !path.EndsWith(".prefab"))
                {
                    return null;
                }
            }
            else if (PrefabStageUtility.GetCurrentPrefabStage() != null
                && PrefabStageUtility.GetCurrentPrefabStage().assetPath.Contains(gameObject.name))
            {
                path = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
            }
            else
            {
                path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                if (!path.IsNullOrEmpty() && !path.EndsWith(".prefab"))
                {
                    Debug.LogError($"The prefab asset at {path} is not of correct format (it should end in .prefab).");
                    return null;
                }
            }

            return path.IsNullOrEmpty() ? null : path;
        }

        /// <summary>
        /// Gets the addressable key for the specified gameobject's prefab asset. If not addressable, it will be made addressable.
        /// </summary>
        public static string GetAddressableKeyFromGameObject(GameObject gameObject)
        {
            string assetPath = GetPathToPrefabAsset(gameObject);
            if (assetPath == null)
            {
                return null;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            const string groupName = "ChunkObjects";
            const string chunkObjectSuffix = "_ChunkObject";
            
            AddressableAssetGroup group = settings.FindGroup(groupName);
            
            AddressableAssetEntry assetEntry = settings.CreateOrMoveEntry(guid, group);
            assetEntry.address = $"{assetPath}{chunkObjectSuffix}";
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, assetEntry, true);
            AssetDatabase.SaveAssets();
            
            return assetEntry.address;
        }

        private static string GetPathToNearestPrefab(GameObject gameObject)
        {
            Transform parent = gameObject.transform;
            while (parent != null)
            {
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(parent);
                if (path.EndsWith(".prefab"))
                    return path;
                
                parent = parent.parent;
            }

            return null;
        }
#endif
        
    }
}
