using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CreateLiveryTexturePanel : AnimatedPanel
    {

        [SerializeField] private Transform optionButtonHolder;
        [SerializeField] private DecalOptionButton optionButtonPrefab;
        [SerializeField] private DecalCategoryHeaderFilter headerFilter;
        [SerializeField] private MultiImageButton selectButton;
        
        [SerializeField, ReadOnly] private DecalOptionButton selectedOption;

        public void PopulateDecals()
        {
            foreach (Transform child in optionButtonHolder)
                child.gameObject.Pool();

            foreach (DecalTexture decalTexture in headerFilter.CurrentSelected.DecalTextures)
            {
                DecalOptionButton instance = optionButtonPrefab.gameObject.GetSpareOrCreate<DecalOptionButton>(optionButtonHolder);
                instance.Initialise(decalTexture);
                instance.transform.SetAsLastSibling();
            }
        }
        
        public void SelectDecalButton(DecalOptionButton option)
        {
            if (selectedOption != null)
                selectedOption.OnDeselect();
            
            selectedOption = option;
            if (selectedOption != null)
                selectedOption.OnSelect();

            selectButton.interactable = selectedOption != null;
        }

        public void OnClickSelectButton()
        {
            Hide();
            
            LiveDecal decal = DecalEditor.Instance.CreateLiveDecal(headerFilter.CurrentSelected, selectedOption.DecalTexture);
            DecalStateManager.LogStateChange(new DecalStateManager.CreateStateChange(decal));
            DecalEditor.Instance.SelectLiveDecal(decal);
        }
        
    }
}
