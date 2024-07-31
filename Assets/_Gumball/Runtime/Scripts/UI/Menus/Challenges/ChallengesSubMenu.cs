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
        
        public override void Show()
        {
            base.Show();

            categoryButton.image.color = selectedCategoryButtonColor;
        }

        public override void Hide()
        {
            base.Hide();
            
            categoryButton.image.color = deselectedCategoryButtonColor;
        }
        
    }
}
