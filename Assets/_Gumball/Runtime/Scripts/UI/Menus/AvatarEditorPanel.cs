using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class AvatarEditorPanel : AnimatedPanel
    {

        [SerializeField] private AvatarCosmeticSelector cosmeticSelector;
        [SerializeField] private AvatarCosmeticDisplay cosmeticDisplay;

        public AvatarCosmeticSelector CosmeticSelector => cosmeticSelector;
        public AvatarCosmeticDisplay CosmeticDisplay => cosmeticDisplay;

        public void OnClickBackButton()
        {
            AvatarEditor.Instance.EndSession();
            MainSceneManager.LoadMainScene();
        }

        public void OnClickDriverSwitch()
        {
            AvatarEditor.Instance.SelectAvatar(true);
        }

        public void OnClickCoDriverSwitch()
        {
            AvatarEditor.Instance.SelectAvatar(false);
        }

        public void OnClickApparelCategory()
        {
            cosmeticSelector.SelectCategory(AvatarCosmeticCategory.Apparel);
        }

    }
}
