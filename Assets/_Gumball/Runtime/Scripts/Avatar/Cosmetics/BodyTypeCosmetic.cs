using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class BodyTypeCosmetic : AvatarCosmetic
    {
        
        [Serializable]
        public struct BodyTypeCosmeticIcon
        {
            [SerializeField] private AvatarBodyType type;
            [SerializeField] private Sprite icon;

            public AvatarBodyType Type => type;
            public Sprite Icon => icon;
        }
        
        [SerializeField] private BodyTypeCosmeticIcon[] options;

        public override int GetMaxIndex() => options.Length - 1;

        public BodyTypeCosmeticIcon[] Options => options;

        public override void OnCreateScrollItem(ScrollItem scrollItem, int index)
        {
            scrollItem.onLoad += () =>
            {
                scrollItem.CurrentIcon.ImageComponent.color = Color.white;
                scrollItem.CurrentIcon.ImageComponent.sprite = options[index].Icon;
            };
            scrollItem.onSelect += () =>
            {
                Apply(index);
            };
        }

        protected override void OnApplyCosmetic(int index)
        {
            avatarBelongsTo.ChangeBodyType(options[index].Type);
        }

        public override int GetSavedIndex()
        {
            return defaultIndex;
        }

        public override void Save()
        {
            avatarBelongsTo.SavedBodyType = options[defaultIndex].Type;
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public void SetNextTesting()
        {
            int newIndex = currentIndex + 1;
            if (newIndex >= options.Length)
                newIndex = 0;
            
            Apply(newIndex);
            Save();
        }
#endif

    }
}
