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

        public static readonly int SkinTintProperty = Shader.PropertyToID("_Skin_Tint");
        
        [SerializeField] private Color[] skinColors;
        [SerializeField] private Sprite circleSprite;

        public Color[] SkinColors => skinColors;

        public override int GetMaxIndex() => skinColors.Length - 1;

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.sprite = circleSprite;
                scrollItem.CurrentIcon.ImageComponent.color = skinColors[index];
            };
            scrollItem.onSelect += () =>
            {
                Apply(index);
            };
        }

        public HashSet<Material> GetMaterialsToEffect()
        {
            HashSet<Material> materials = new HashSet<Material>();
            foreach (Material material in avatarBelongsTo.CurrentBody.AttachedMaterials)
            {
                if (material.HasProperty(SkinTintProperty))
                    materials.Add(material);
            }

            return materials;
        }
        
        protected override void OnApplyCosmetic(int index)
        {
            foreach (Material material in GetMaterialsToEffect())
            {
                material.SetColor(SkinTintProperty, skinColors[index].WithAlphaSetTo(1));
            }
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public void SetNextTesting()
        {
            int newIndex = currentIndex + 1;
            if (newIndex >= skinColors.Length)
                newIndex = 0;
            
            Apply(newIndex);
            Save();
        }
#endif

    }
}
