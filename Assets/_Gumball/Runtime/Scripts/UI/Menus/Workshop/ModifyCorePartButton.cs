using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ModifyCorePartButton : MonoBehaviour
    {

        [SerializeField] private Image glow;
        [SerializeField] private GlobalColourPalette.ColourCode selectedColor;
        [SerializeField] private GlobalColourPalette.ColourCode deselectedColor;

        public void Select()
        {
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(selectedColor);
        }

        public void Deselect()
        {
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(deselectedColor);
        }
        
    }
}
