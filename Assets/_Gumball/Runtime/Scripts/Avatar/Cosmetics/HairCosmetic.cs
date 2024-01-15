using System;
using System.Collections;
using System.Collections.Generic;
using CC;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class HairCosmetic : AvatarCosmetic
    {

        [Serializable]
        public struct HairItemData
        {
            [SerializeField] private AssetReferenceGameObject prefab;
            [SerializeField] private Sprite icon;
            [SerializeField] private Texture2D shadowMap;
            [SerializeField] private bool addCopyPose;

            public AssetReferenceGameObject Prefab => prefab;
            public Sprite Icon => icon;
            public Texture2D ShadowMap => shadowMap;
            public bool AddCopyPose => addCopyPose;
        }
        
        [SerializeField] private List<HairItemData> items = new();
        [SerializeField] private string shadowMapProperty;
        
        [Foldout("Debugging"), SerializeField, ReadOnly] private GameObject currentItem;

        public List<HairItemData> Items => items;
        public GameObject CurrentItem => currentItem;

        public override int GetMaxIndex()
        {
            return items.Count - 1;
        }

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.sprite = items[index].Icon;
                scrollItem.CurrentIcon.ImageComponent.color = Color.white;
            };
            scrollItem.onSelect += () =>
            {
                Apply(index);
            };
        }

        protected override void OnApplyCosmetic(int index)
        {
            //destroy the current
            if (currentItem != null)
                Destroy(currentItem);

            HairItemData itemData = items[index];
            if (itemData.Prefab.RuntimeKeyIsValid())
            {
                //instantiate the mesh
                currentItem = InstantiatePrefab(itemData.Prefab);
                
                //add blend shapes
                AddBlendShapes(currentItem);
                
                //assign bones
                AssignBones(currentItem, itemData.AddCopyPose);
                
                //set the shadow map texture
                SetShadowMap(itemData);
            }
            else
            {
                currentItem = null;
                
                //remove the shadow map
                SetShadowMap(itemData);
            }
        }

        public override HashSet<Material> GetMaterialsWithColorProperty()
        {
            HashSet<Material> materials = base.GetMaterialsWithColorProperty();

            if (currentItem == null)
                return materials;
            
            //also add any materials on the current item
            
            foreach (Transform child in currentItem.transform.GetComponentsInAllChildren<Transform>())
            {
                SkinnedMeshRenderer meshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if (meshRenderer == null)
                    continue;
                
                foreach (Material material in meshRenderer.materials)
                {
                    foreach (string property in ColorMaterialProperties)
                    {
                        if (material.HasProperty(property))
                        {
                            materials.Add(material);
                            break;
                        }
                    }
                }
            }

            return materials;
        }

        private GameObject InstantiatePrefab(AssetReferenceGameObject prefab)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(prefab);
            handle.WaitForCompletion();

            GameObject item = Instantiate(handle.Result, transform);
            item.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);

            return item;
        }
        
        private void AddBlendShapes(GameObject item)
        {
            //Add blendshape managers and update shapes
            foreach (SkinnedMeshRenderer mesh in item.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                BlendshapeManager manager = mesh.gameObject.AddComponent<BlendshapeManager>();
                manager.parseBlendshapes();
                foreach (CC_Property shapeData in avatarBelongsTo.CurrentBody.Customiser.StoredCharacterData.Blendshapes)
                {
                    manager.setBlendshape(shapeData.propertyName, shapeData.floatValue);
                }
            }
        }
        
        private void AssignBones(GameObject item, bool addCopyPose)
        {
            if (addCopyPose)
            {
                item.AddComponent<CopyPose>().Initialise(avatarBelongsTo.CurrentBody.transform);
                return;
            }
            
            foreach (SkinnedMeshRenderer mesh in item.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var MyBones = new Transform[mesh.bones.Length];
                for (var i = 0; i < mesh.bones.Length; i++)
                {
                    if (mesh.bones[i] == null) continue;
                    Transform rootBone = avatarBelongsTo.CurrentBody.Customiser.MainMesh.rootBone;
                    MyBones[i] = rootBone.FindChildByName(mesh.bones[i].name);
                }
                mesh.bones = MyBones;
            }
        }
        
        private void SetShadowMap(HairItemData itemData)
        {
            foreach (Material material in GetMaterialsWithShadowMap())
            {
                material.SetTexture(shadowMapProperty, itemData.ShadowMap);
            }
        }
        
        private HashSet<Material> GetMaterialsWithShadowMap()
        {
            HashSet<Material> materials = new HashSet<Material>();
            foreach (Material material in avatarBelongsTo.CurrentBody.AttachedMaterials)
            {
                if (material.HasProperty(shadowMapProperty))
                    materials.Add(material);
            }

            return materials;
        }
        
    }
}
