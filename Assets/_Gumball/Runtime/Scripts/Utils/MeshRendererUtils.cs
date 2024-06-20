using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalOpus.MB.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    public static class MeshRendererUtils
    {

        public static bool HasValidMaterials(this Material[] materials)
        {
            if (materials == null)
                return false;

            if (materials.Length == 0)
                return false;

            if (!HasValidMaterialInArray(materials))
                return false;

            return true;
        }

        private static bool HasValidMaterialInArray(Material[] materials)
        {
            foreach (Material material in materials)
            {
                if (material != null)
                    return true;
            }

            return false;
        }
        
#if UNITY_EDITOR
        public enum CombineMeshCleanup
        {
            NONE,
            DISABLE,
            DESTROY
        }
        
        /// <summary>
        /// Combines the GameObjects into a prefab and returns the prefab.
        /// </summary>
        public static GameObject CombineMeshesIntoPrefab(List<GameObject> gameObjects, string prefabPath, CombineMeshCleanup cleanup = CombineMeshCleanup.NONE)
        {
            if (gameObjects.Count == 0)
            {
                Debug.LogError("Cannot combine meshes because there weren't any gameobjects supplied.");
                return null;
            }
            
            //create the meshbaker
            MB3_MeshBaker meshBaker = new GameObject("MeshBaker").AddComponent<MB3_MeshBaker>();
            
            //bake into a prefab
            meshBaker.meshCombiner.outputOption = MB2_OutputOptions.bakeIntoPrefab;
            
            //add the objects to the baker
            meshBaker.useObjsToMeshFromTexBaker = false;
            meshBaker.objsToMesh = gameObjects;
            
            //settings for reducing file size
            meshBaker.meshCombiner.settings.doTan = false; //will need to reenable if the mesh uses bump maps
            meshBaker.meshCombiner.settings.clearBuffersAfterBake = true;

            //check if the result prefab already exists, if yes - use it - else create one
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                //create the prefab asset if it hasn't been created before
                GameObject temp = new GameObject();
                PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
                
                Object.DestroyImmediate(temp);
                
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
            
            meshBaker.resultPrefab = prefab;

            //bake
            MB3_MeshBakerEditorFunctions.BakeIntoCombined(meshBaker, out _);
            
            //delete the mesh baker
            Object.DestroyImmediate(meshBaker.gameObject);

            //cleanup objects
            if (cleanup != CombineMeshCleanup.NONE)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    if (cleanup == CombineMeshCleanup.DISABLE)
                        gameObject.SetActive(false);
                    if (cleanup == CombineMeshCleanup.DESTROY)
                        Object.DestroyImmediate(gameObject);
                }
            }

            return meshBaker.resultPrefab;
        }
#endif
        
    }
}
