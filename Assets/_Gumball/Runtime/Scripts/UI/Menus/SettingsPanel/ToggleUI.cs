using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class ToggleUI : MonoBehaviour
    {

        public event Action<bool> onToggleChange;

        [SerializeField] private Image selectorIcon;
        [SerializeField] private float selectorIconOffset = 25;
        [SerializeField] private TextMeshProUGUI leftSideLabel;
        [SerializeField] private TextMeshProUGUI rightSideLabel;
        [SerializeField] private GlobalColourPalette.ColourCode deselectedLabelColourCode = GlobalColourPalette.ColourCode.C3;
        [SerializeField] private GlobalColourPalette.ColourCode selectedLabelColourCode = GlobalColourPalette.ColourCode.C13;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool rightSideEnabled;
        
        private bool isInitialised;
        
        private Color selectedLabelColor => GlobalColourPalette.Instance.GetGlobalColor(selectedLabelColourCode);
        private Color deselectedLabelColor => GlobalColourPalette.Instance.GetGlobalColor(deselectedLabelColourCode);
        
        public bool RightSideEnabled => rightSideEnabled;

        private void OnEnable()
        {
            if (!isInitialised)
            {
                isInitialised = true;
                Initialise();
            }
        }
        
        public void OnClickToggle()
        {
            SetToggle(!rightSideEnabled);
        }
        
        public void SetToggle(bool rightSideEnabled)
        {
            bool previousValue = this.rightSideEnabled;
            this.rightSideEnabled = rightSideEnabled;
            
            selectorIcon.rectTransform.anchoredPosition = selectorIcon.rectTransform.anchoredPosition.SetX(rightSideEnabled ? selectorIconOffset : -selectorIconOffset);
            leftSideLabel.color = rightSideEnabled ? deselectedLabelColor : selectedLabelColor;
            rightSideLabel.color = rightSideEnabled ? selectedLabelColor : deselectedLabelColor;
            
            if (rightSideEnabled)
                OnSelectRightSide();
            else
                OnSelectLeftSide();
            
            if (rightSideEnabled != previousValue)
                onToggleChange?.Invoke(rightSideEnabled);
        }
        
        protected abstract void Initialise();

        protected abstract void OnSelectLeftSide();
        
        protected abstract void OnSelectRightSide();

    }
}
