using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public abstract class BlendShapeCosmetic : AvatarCosmetic
    {

        [Serializable]
        public struct PropertyModifier
        {
            [SerializeField] public string property;
            [SerializeField] public float value;

            public string Property => property;
            public float Value => value;
            
            public void Apply(AvatarBody body)
            {
                body.SetBlendshape(property, value);
            }
        }

        [Serializable]
        public struct BlendShapeOption
        {
            [SerializeField] private string name;
            [SerializeField] private Sprite icon;
            [SerializeField] private PropertyModifier[] propertyModifiers;

            public BlendShapeOption(BlendShapeOption optionToCopy)
            {
                name = optionToCopy.name;
                propertyModifiers = optionToCopy.propertyModifiers.ToArray();
                icon = optionToCopy.icon;
            }
            
            public PropertyModifier[] PropertyModifiers => propertyModifiers;
            public Sprite Icon => icon;
        }
        
        [SerializeField] protected BlendShapeOption[] options;

        public BlendShapeOption[] Options => options;
        
        public override int GetMaxIndex() => options.Length - 1;

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.enabled = true;
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
            foreach (PropertyModifier propertyModifier in options[index].PropertyModifiers)
            {
                propertyModifier.Apply(avatarBelongsTo.CurrentBody);  
            }
        }
        
    }
}
