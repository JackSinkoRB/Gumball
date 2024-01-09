using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SkinColourCosmetic : AvatarCosmetic
    {

        private static readonly int SkinTintProperty = Shader.PropertyToID("_Skin_Tint");
        
        [SerializeField] private Color[] colors;
        [SerializeField] private Sprite circleSprite;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Material[] materialsToEffectCached;

        private Material[] materialsToEffect
        {
            get
            {
                if (materialsToEffectCached == null || materialsToEffectCached.Length == 0)
                {
                    List<Material> materials = new List<Material>();
                    foreach (SkinnedMeshRenderer mesh in avatarBelongsTo.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        foreach (Material material in mesh.materials)
                        {
                            materials.Add(material);
                        }
                    }

                    materialsToEffectCached = materials.ToArray();
                }

                return materialsToEffectCached;
            }
        }
        
        public override int GetMaxIndex() => colors.Length - 1;

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.sprite = circleSprite;
                scrollItem.CurrentIcon.ImageComponent.color = colors[index];
            };
        }

        protected override void OnApplyCosmetic(int index)
        {
            foreach (Material material in materialsToEffect)
            {
                material.SetColor(SkinTintProperty, colors[index].WithAlphaSetTo(1));
            }
        }
        
#if UNITY_EDITOR
        [ButtonMethod]
        public void SetNextTesting()
        {
            int newIndex = currentIndex + 1;
            if (newIndex >= colors.Length)
                newIndex = 0;
            
            Apply(newIndex);
            SaveData();
        }
#endif

    }
}
