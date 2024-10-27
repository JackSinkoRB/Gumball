using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalOptionButton : MonoBehaviour
    {
        
        [SerializeField] private Image icon;
        [SerializeField] private GameObject frame;
        [Space(5)]
        [SerializeField] private Image glow;
        [SerializeField] private GlobalColourPalette.ColourCode glowSelectedColor;
        [SerializeField] private GlobalColourPalette.ColourCode glowDeselectedColor;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private DecalTexture decalTexture;

        private Button button => GetComponent<Button>();

        public DecalTexture DecalTexture => decalTexture;
        
        public void Initialise(DecalTexture decalTexture)
        {
            this.decalTexture = decalTexture;
            
            icon.sprite = decalTexture == null ? null : decalTexture.Sprite;
            
            OnDeselect();
        }

        public void OnClickButton()
        {
            PanelManager.GetPanel<CreateLiveryTexturePanel>().SelectDecalButton(this);
        }
        
        public void OnSelect()
        {
            frame.SetActive(true);
            
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(glowSelectedColor);
        }

        public void OnDeselect()
        {
            frame.SetActive(false);
            
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(glowDeselectedColor);
        }
        
    }
}
