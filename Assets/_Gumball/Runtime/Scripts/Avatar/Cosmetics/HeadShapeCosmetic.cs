using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class HeadShapeCosmetic : AvatarCosmetic
    {

        [Serializable]
        public struct PropertyModifier
        {
            [SerializeField] private string property;
            [SerializeField] private float value;

            public string Property => property;
            public float Value => value;
            
            public void Apply(AvatarBody body)
            {
                body.SetBlendshape(property, value);
            }
        }

        [Serializable]
        public struct HeadShapeOption
        {
            [SerializeField] private string name;
            [SerializeField] private PropertyModifier[] propertyModifiers;
            [SerializeField] private Sprite icon;

            public PropertyModifier[] PropertyModifiers => propertyModifiers;
            public Sprite Icon => icon;
        }
        
        [SerializeField] private HeadShapeOption[] options;

        public HeadShapeOption[] Options => options;

        public override int GetMaxIndex() => options.Length - 1;

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
            foreach (PropertyModifier propertyModifier in options[index].PropertyModifiers)
            {
                propertyModifier.Apply(avatarBelongsTo.CurrentBody);  
            }
        }

    }
}
