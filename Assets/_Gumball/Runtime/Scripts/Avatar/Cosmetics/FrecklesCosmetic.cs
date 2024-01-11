using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class FrecklesCosmetic : AvatarCosmetic
    {
        
        [Serializable]
        public struct FrecklesCosmeticIcon
        {
            [SerializeField] private float value;
            [SerializeField] private Sprite icon;

            public float Value => value;
            public Sprite Icon => icon;
        }

        public static readonly int FrecklesProperty = Shader.PropertyToID("_Freckles");
        
        [SerializeField] private FrecklesCosmeticIcon[] options;

        public FrecklesCosmeticIcon[] Options => options;
        
        public HashSet<Material> MaterialsToEffect
        {
            get
            {
                HashSet<Material> materials = new HashSet<Material>();
                foreach (SkinnedMeshRenderer mesh in avatarBelongsTo.CurrentBody.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    foreach (Material material in mesh.materials)
                    {
                        if (material.HasProperty(FrecklesProperty))
                            materials.Add(material);
                    }
                }

                return materials;
            }
        }
        
        public override int GetMaxIndex()
        {
            return options.Length - 1;
        }

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.sprite = options[index].Icon;
                scrollItem.CurrentIcon.ImageComponent.color = Color.white;
            };
            scrollItem.onSelect += () =>
            {
                Apply(index);
            };
        }

        protected override void OnApplyCosmetic(int index)
        {
            foreach (Material material in MaterialsToEffect)
            {
                material.SetFloat(FrecklesProperty, options[index].Value);
            }
        }
    }
}
