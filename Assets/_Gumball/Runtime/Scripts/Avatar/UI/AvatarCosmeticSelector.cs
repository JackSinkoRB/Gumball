using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class AvatarCosmeticSelector : SwitchButton
    {
        
        public static event Action<AvatarCosmetic> onSelectCosmetic;
        public static event Action onDeselectCosmetic;

        [SerializeField] private AvatarCosmeticPanel cosmeticPanel;
        [SerializeField] private GridLayoutWithScreenSize cosmeticButtonsHolder;
        [SerializeField] private GameObject cosmeticButtonPrefab;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            onSelectCosmetic = null;
            onDeselectCosmetic = null;
        }
        
        private void OnEnable()
        {
            AvatarEditor.onSelectedAvatarChanged += OnSelectedAvatarChanged;
            Avatar.onChangeBodyType += OnBodyTypeChanged;
            PrimaryContactInput.onRelease += OnPressRelease;
        }

        private void OnDisable()
        {
            AvatarEditor.onSelectedAvatarChanged -= OnSelectedAvatarChanged;
            Avatar.onChangeBodyType -= OnBodyTypeChanged;
            PrimaryContactInput.onRelease -= OnPressRelease;
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
            PopulateCategoryButtons(category);

            //also populate the first cosmetic for the category
            Dictionary<AvatarCosmeticCategory, List<AvatarCosmetic>> cosmetics = AvatarEditor.Instance.CurrentSelectedAvatar.CurrentBody.CosmeticsGrouped;
            AvatarCosmetic firstCosmeticInCategory = cosmetics.ContainsKey(category) && cosmetics[category].Count > 0 ? cosmetics[category][0] : null;
            SelectCosmetic(firstCosmeticInCategory);
            
            SetButtonSelected(category == AvatarCosmeticCategory.Body);
        }

        public void SelectCosmetic(AvatarCosmetic cosmetic)
        {
            cosmeticPanel.PopulateCosmeticOptions(cosmetic);
            cosmeticPanel.Show();
            onSelectCosmetic?.Invoke(cosmetic);
        }

        private void PopulateCategoryButtons(AvatarCosmeticCategory category)
        {
            Dictionary<AvatarCosmeticCategory, List<AvatarCosmetic>> cosmetics = AvatarEditor.Instance.CurrentSelectedAvatar.CurrentBody.CosmeticsGrouped;
            
            //hide previous
            foreach (Transform child in cosmeticButtonsHolder.transform)
                child.gameObject.Pool();
            
            if (cosmetics.ContainsKey(category))
            {
                foreach (AvatarCosmetic cosmetic in cosmetics[category])
                {
                    AvatarEditorCosmeticButton buttonInstance = cosmeticButtonPrefab.GetSpareOrCreate<AvatarEditorCosmeticButton>(cosmeticButtonsHolder.transform);
                    buttonInstance.Initialise(this, cosmetic);
                    
                    buttonInstance.transform.SetAsLastSibling();
                }
            }
            
            cosmeticButtonsHolder.Resize();
        }

        private void DeselectCosmetic()
        {
            cosmeticPanel.Hide();
            
            onDeselectCosmetic?.Invoke();
        }
        
        private void OnPressRelease()
        {
            CheckToDeselectCosmetics();
        }
        
        private void CheckToDeselectCosmetics()
        {
            bool positionHasMoved = !PrimaryContactInput.OffsetSincePressedNormalised.Approximately(Vector2.zero, PrimaryContactInput.PressedThreshold);
            if (positionHasMoved)
                return;
            
            //check if click is blocked by UI
            if (PrimaryContactInput.IsGraphicUnderPointer())
                return;
            
            if (cosmeticPanel.SelectedCosmetic == null)
            {
                DeselectCosmetic();
                return;
            }
            
            DeselectCosmetic();
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
