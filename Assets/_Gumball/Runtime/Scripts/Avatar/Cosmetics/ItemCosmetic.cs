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
    /// <summary>
    /// A cosmetic that has an item that attaches to the avatar.
    /// </summary>
    public abstract class ItemCosmetic : AvatarCosmetic
    {

        [Serializable]
        public struct ItemData
        {
            [SerializeField] private AssetReferenceGameObject prefab;
            [SerializeField] private Sprite icon;
            [SerializeField] private Texture2D mask;
            [SerializeField] private bool addCopyPose;
            [SerializeField] private FootOffset footOffset;
            [SerializeField] private ColorableCosmeticOption colorable;

            public AssetReferenceGameObject Prefab => prefab;
            public Sprite Icon => icon;
            public Texture2D Mask => mask;
            public bool AddCopyPose => addCopyPose;
            public FootOffset FootOffset => footOffset;
            public ColorableCosmeticOption Colorable => colorable;
        }

        [SerializeField] private List<ItemData> items = new();
        [SerializeField] private string maskProperty;

        [Foldout("Debugging"), SerializeField, ReadOnly]
        private GameObject currentItem;
        [Foldout("Debugging"), SerializeField, ReadOnly]
        private int currentColorIndex = -1;

        public List<ItemData> Items => items;
        public ItemData CurrentItemData => currentIndex == -1 ? items[defaultIndex] : items[currentIndex];
        public GameObject CurrentItem => currentItem;
        public int CurrentColorIndex => currentColorIndex;
        
        private string colorSaveKey => $"{saveKey}.SelectedColorIndex";

        public override int GetMaxIndex()
        {
            return items.Count - 1;
        }

        public override void Save()
        {
            base.Save();
            
            DataManager.Avatar.Set(colorSaveKey, currentColorIndex);
        }
        
        public int GetSavedColorIndex()
        {
            return DataManager.Avatar.Get(colorSaveKey, -1);
        }

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.sprite = items[index].Icon;
                scrollItem.CurrentIcon.ImageComponent.color = items[index].Icon == null ? Color.white.WithAlphaSetTo(0) : Color.white;
            };
            scrollItem.onSelect += () =>
            {
                Apply(index);
            };
        }

        public HashSet<Material> GetMaterialsWithColorProperty()
        {
            HashSet<Material> materials = new HashSet<Material>();
            foreach (Material material in avatarBelongsTo.CurrentBody.AttachedMaterials)
            {
                foreach (string property in CurrentItemData.Colorable.ColorMaterialProperties)
                {
                    if (material.HasProperty(property))
                    {
                        materials.Add(material);
                        break;
                    }
                }
            }
            
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
                    foreach (string property in CurrentItemData.Colorable.ColorMaterialProperties)
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

        protected override void OnApplyCosmetic(int index)
        {
            //destroy the current
            if (currentItem != null)
                Destroy(currentItem);

            ItemData itemData = items[index];
            
            if (itemData.Prefab.RuntimeKeyIsValid())
            {
                //instantiate the mesh
                currentItem = InstantiatePrefab(itemData.Prefab);
                
                //add blend shapes
                AddBlendShapes(currentItem);
                
                //assign bones
                AssignBones(currentItem, itemData.AddCopyPose);
                
                //set foot offset
                SetFootOffset(itemData);
            }
            else
            {
                currentItem = null;
            }
            
            //set the mask textures
            SetMasks(itemData);
            
            //set the colour
            ApplyDefaultColor(itemData);
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
                foreach (CC_Property shapeData in avatarBelongsTo.CurrentBody.Customiser.StoredCharacterData.Blendshapes)
                {
                    manager.SetBlendshape(shapeData.propertyName, shapeData.floatValue);
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

        private void SetFootOffset(ItemData itemData)
        {
            if (!itemData.FootOffset.IsUsed)
                return;
            
            //TODO: the bones can be cached in the avatar
            foreach (TransformBone component in avatarBelongsTo.GetComponentsInChildren<TransformBone>())
            {
                component.SetOffset(itemData.FootOffset);
            }
        }

        private void SetMasks(ItemData itemData)
        {
            foreach (Material material in avatarBelongsTo.CurrentBody.GetMaterialsWithProperty(maskProperty))
            {
                material.SetTexture(maskProperty, itemData.Mask);
            }
        }

        private void ApplyDefaultColor(ItemData itemData)
        {
            if (!itemData.Colorable.IsColorable)
                return;

            if (currentColorIndex == -1)
                currentColorIndex = GetSavedColorIndex();

            if (currentColorIndex == -1
                || currentColorIndex >= itemData.Colorable.Colors.Length)
                currentColorIndex = itemData.Colorable.DefaultColorIndex;

            ApplyColor(currentColorIndex);
        }

        public void ApplyColor(int index)
        {
            if (!CurrentItemData.Colorable.IsColorable)
                return;
            
            currentColorIndex = index;

            foreach (Material material in GetMaterialsWithColorProperty())
            {
                foreach (string property in CurrentItemData.Colorable.ColorMaterialProperties)
                {
                    material.SetColor(property, CurrentItemData.Colorable.Colors[index]);
                }
            }
        }

    }
}
