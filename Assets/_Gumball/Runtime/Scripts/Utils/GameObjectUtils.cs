using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using MyBox;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

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
            if (PrefabStageUtility.GetCurrentPrefabStage() != null
                && PrefabStageUtility.GetCurrentPrefabStage().assetPath.Contains(gameObject.name))
            {
                path = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
            }
            else
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                path = AssetDatabase.GetAssetPath(prefab);
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
            AddressableAssetEntry assetEntry = settings.FindAssetEntry(guid);
            if (assetEntry == null)
            {
                Debug.Log($"Could not find asset entry for {gameObject.name}. Creating one.");
                const string groupName = "ChunkObjects";
                const string chunkObjectSuffix = "_ChunkObject";

                AddressableAssetGroup group = settings.FindGroup(groupName);
            
                assetEntry = settings.CreateOrMoveEntry(guid, group);
                assetEntry.address = $"{gameObject.name}{chunkObjectSuffix}";
            
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, assetEntry, true);
                AssetDatabase.SaveAssets();
            }
            
            return assetEntry.address;
        }
#endif
        
    }
}
