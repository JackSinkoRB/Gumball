using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class ChallengesSubMenu : SubMenu
    {

        [SerializeField] private Button categoryButton;
        [SerializeField] private Color selectedCategoryButtonColor = Color.white;
        [SerializeField] private Color deselectedCategoryButtonColor = Color.white;

        protected override void OnShow()
        {
            base.OnShow();
            
            categoryButton.image.color = selectedCategoryButtonColor;
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            categoryButton.image.color = deselectedCategoryButtonColor;
        }
        
    }
}
