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

        /// <summary>
        /// Checks if the transform has the component T enabled, or if any of the children have it enabled.
        /// </summary>
        public static bool HasActiveComponentsInChildren<T>(this GameObject gameObject)
        {
            T self = gameObject.GetComponent<T>();
            if (self != null)
                return true;

            foreach (T child in gameObject.transform.GetComponentsInAllChildren<T>())
            {
                if (child != null)
                    return true;
            }

            return false;
        }
        
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
        /// Checks whether the gameobject is empty, and all child gameobjects and childs children etc. are also empty.
        /// </summary>
        public static bool IsCompletelyEmpty(this GameObject gameObject)
        {
            // Helper function to check a single transform recursively
            bool IsTransformEmpty(Transform currentTransform)
            {
                // Get all components attached to the transform
                Component[] components = currentTransform.GetComponents<Component>();
        
                // If the transform has more than one component (i.e., components other than Transform itself), it's not empty
                if (components.Length > 1)
                {
                    return false;
                }

                // Recursively check each child transform
                foreach (Transform child in currentTransform)
                {
                    if (!IsTransformEmpty(child))
                    {
                        return false;
                    }
                }

                // If this transform and all its children are empty, return true
                return true;
            }

            // Start the recursive check from the provided transform
            return IsTransformEmpty(gameObject.transform);
        }
        
        /// <summary>
        /// Returns the total number of children gameobjects under the given transform,
        /// including all children, children's children, and so on.
        /// </summary>
        public static int GetTotalChildCount(this GameObject gameObject)
        {
            // Helper function to recursively count children
            int CountChildren(Transform currentTransform)
            {
                int count = 0;

                // Iterate through each child transform
                foreach (Transform child in currentTransform)
                {
                    // Increment count for this child
                    count++;
                    // Recursively count this child's children
                    count += CountChildren(child);
                }

                return count;
            }

            // Start the recursive count from the provided transform
            return CountChildren(gameObject.transform);
        }
        
        /// <summary>
        /// Gets the addressable key for the specified gameobject's prefab asset. If not addressable, it will be made addressable.
        /// </summary>
        public static string GetOrSetAddressableKeyFromGameObject(GameObject gameObject, string addressableGroup, string addressSuffix = "", bool saveAssets = true)
        {
            string assetPath = GetPathToPrefabAsset(gameObject);
            if (assetPath == null)
            {
                return null;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            AddressableAssetGroup group = settings.FindGroup(addressableGroup);
            
            AddressableAssetEntry assetEntry = settings.CreateOrMoveEntry(guid, group);
            assetEntry.address = $"{assetPath}{addressSuffix}";
            
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, assetEntry, true);
            if (saveAssets)
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
