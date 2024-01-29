using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class AvatarCosmeticSelector : SwitchButton
    {

        [SerializeField] private AvatarCosmeticDisplay cosmeticDisplay;
        [SerializeField] private MagneticScroll magneticScroll;

        private void OnEnable()
        {
            AvatarEditor.onSelectedAvatarChanged += OnSelectedAvatarChanged;
            Avatar.onChangeBodyType += OnBodyTypeChanged;
        }

        private void OnDisable()
        {
            AvatarEditor.onSelectedAvatarChanged -= OnSelectedAvatarChanged;
            Avatar.onChangeBodyType -= OnBodyTypeChanged;
        }

        public override void OnClickLeftSwitch()
        {
            base.OnClickLeftSwitch();
            
            SelectCategory(AvatarCosmeticCategory.Body);
        }

        public override void OnClickRightSwitch()
        {
            base.OnClickRightSwitch();
            
            SelectCategory(AvatarCosmeticCategory.Apparel);
        }

        public void SelectCategory(AvatarCosmeticCategory category)
        {
            Dictionary<AvatarCosmeticCategory, List<AvatarCosmetic>> cosmetics = AvatarEditor.Instance.CurrentSelectedAvatar.CurrentBody.CosmeticsGrouped;
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            if (cosmetics.ContainsKey(category))
            {
                foreach (AvatarCosmetic cosmetic in cosmetics[category])
                {
                    ScrollItem scrollItem = new ScrollItem();
                    scrollItem.onLoad += () => scrollItem.CurrentIcon.ImageComponent.sprite = cosmetic.Icon;
                    scrollItem.onSelect += () => cosmeticDisplay.PopulateCosmeticOptions(cosmetic);
                    scrollItems.Add(scrollItem);
                }
            }

            magneticScroll.SetItems(scrollItems);

            //also populate the first cosmetic for the category
            AvatarCosmetic firstCosmeticInCategory = cosmetics.ContainsKey(category) && cosmetics[category].Count > 0 ? cosmetics[category][0] : null;
            cosmeticDisplay.PopulateCosmeticOptions(firstCosmeticInCategory);

            SetButtonSelected(category == AvatarCosmeticCategory.Body);
        }
        
        private void OnBodyTypeChanged(Avatar avatar, AvatarBodyType previousbodytype, AvatarBodyType newbodytype)
        {
            if (!AvatarEditor.Instance.SessionInProgress)
                return;
            
            SelectCategory(AvatarCosmeticCategory.Body); //repopulate the magnetic scrolls as using different data for different body
        }

        private void OnSelectedAvatarChanged(Avatar oldAvatar, Avatar newAvatar)
        {
            SelectCategory(AvatarCosmeticCategory.Body);
        }

    }
}
