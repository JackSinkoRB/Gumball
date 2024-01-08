using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SkinColourCosmetic : AvatarCosmetic
    {

        private static readonly int SkinTintProperty = Shader.PropertyToID("_Skin_Tint");
        
        [SerializeField] private Color[] colors;
        
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
