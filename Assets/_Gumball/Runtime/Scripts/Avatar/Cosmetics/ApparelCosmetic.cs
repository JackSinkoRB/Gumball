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
    public abstract class ApparelCosmetic : AvatarCosmetic
    {

        [Serializable]
        public struct ApparelItemData
        {
            [SerializeField] private AssetReferenceGameObject prefab;
            [SerializeField] private Sprite icon;
            [SerializeField] private string displayName;
            [SerializeField] private Texture2D mask;
            [SerializeField] private FootOffset footOffset;

            public AssetReferenceGameObject Prefab => prefab;
            public Sprite Icon => icon;
            public string DisplayName => displayName;
            public Texture2D Mask => mask;
            public FootOffset FootOffset => footOffset;
        }

        [SerializeField] private List<ApparelItemData> items = new();
        [SerializeField] private string maskProperty;

        [Foldout("Debugging"), SerializeField, ReadOnly] private GameObject currentItem;

        public List<ApparelItemData> Items => items;
        public GameObject CurrentItem => currentItem;
        
        public HashSet<Material> MaterialsWithMask
        {
            get
            {
                HashSet<Material> materials = new HashSet<Material>();
                foreach (Material material in avatarBelongsTo.CurrentBody.AttachedMaterials)
                {
                    if (material.HasProperty(maskProperty))
                        materials.Add(material);
                }

                return materials;
            }
        }
        
        public override void Initialise(Avatar avatar)
        {
            base.Initialise(avatar);
            
            //TODO: load all the handles in parallel to ensure they're loaded for quicker switching
        }

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
            Destroy(currentItem);

            ApparelItemData itemData = items[index];
            if (itemData.Prefab != null)
            {
                //instantiate the mesh
                currentItem = InstantiatePrefab(itemData.Prefab);
                
                //add blend shapes
                AddBlendShapes(currentItem);
                
                //assign bones
                AssignBones(currentItem);
                
                //set foot offset
                SetFootOffset(itemData);
                
                //set the mask textures
                SetMasks(itemData);
            }
            else
            {
                currentItem = null;
            }
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

        private void AssignBones(GameObject item)
        {
            foreach (var mesh in item.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var MyBones = new Transform[mesh.bones.Length];
                for (var i = 0; i < mesh.bones.Length; i++)
                {
                    if (mesh.bones[i] == null) continue;
                    MyBones[i] = FindChildByName(mesh.bones[i].name, avatarBelongsTo.CurrentBody.Customiser.MainMesh.rootBone);
                }
                mesh.bones = MyBones;
            }
        }

        private void SetFootOffset(ApparelItemData itemData)
        {
            if (itemData.FootOffset.HeightOffset < 0)
                return;
            
            //TODO: the bones can be cached in the avatar
            foreach (TransformBone component in avatarBelongsTo.GetComponentsInChildren<TransformBone>())
            {
                component.SetOffset(itemData.FootOffset);
            }
        }

        private void SetMasks(ApparelItemData itemData)
        {
            foreach (Material material in MaterialsWithMask)
            {
                material.SetTexture(maskProperty, itemData.Mask);
            }
        }

        private Transform FindChildByName(string name, Transform parentObj)
        {
            if (parentObj.name == name)
                return parentObj.transform;
            
            foreach (Transform child in parentObj)
            {
                Transform childOfChild = FindChildByName(name, child);
                if (childOfChild != null)
                    return childOfChild;
            }
            
            return null;
        }

    }
}
