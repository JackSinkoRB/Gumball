using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class PropertyCosmetic : AvatarCosmetic
    {
        
        [Serializable]
        public struct PropertyCosmeticIcon
        {
            [SerializeField] private float value;
            [SerializeField] private Sprite icon;

            public float Value => value;
            public Sprite Icon => icon;
        }

        [SerializeField] private string property;
        [SerializeField] private PropertyCosmeticIcon[] options;
        
        public string Property => property;
        public PropertyCosmeticIcon[] Options => options;
        
        public override int GetMaxIndex()
        {
            return options.Length - 1;
        }

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.sprite = options[index].Icon;
                scrollItem.CurrentIcon.ImageComponent.color = options[index].Icon == null ? Color.white.WithAlphaSetTo(0) : Color.white;
            };
            scrollItem.onSelect += () =>
            {
                Apply(index);
            };
        }

        protected override void OnApplyCosmetic(int index)
        {
            foreach (Material material in avatarBelongsTo.CurrentBody.GetMaterialsWithProperty(property))
            {
                material.SetFloat(property, options[index].Value);
            }
        }
        
    }
}
