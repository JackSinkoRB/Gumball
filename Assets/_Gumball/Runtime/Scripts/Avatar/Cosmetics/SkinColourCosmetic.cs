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

        private HashSet<Material> materialsToEffect
        {
            get
            {
                HashSet<Material> materials = new HashSet<Material>();
                foreach (SkinnedMeshRenderer mesh in avatarBelongsTo.CurrentBody.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    foreach (Material material in mesh.materials)
                    {
                        materials.Add(material);
                    }
                }

                return materials;
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
            scrollItem.onSelect += () =>
            {
                Apply(index);
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
            SaveIndex();
        }
#endif

    }
}
