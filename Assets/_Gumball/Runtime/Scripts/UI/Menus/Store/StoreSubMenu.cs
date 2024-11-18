using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class StoreSubMenu : SubMenu
    {

        [SerializeField] private Transform categoryButtonSelected;
        [SerializeField] private Transform categoryButtonDeselected;

        protected override void OnShow()
        {
            base.OnShow();
            
            categoryButtonDeselected.gameObject.SetActive(false);
            categoryButtonSelected.gameObject.SetActive(true);
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            categoryButtonDeselected.gameObject.SetActive(true);
            categoryButtonSelected.gameObject.SetActive(false);
        }
        
    }
}
