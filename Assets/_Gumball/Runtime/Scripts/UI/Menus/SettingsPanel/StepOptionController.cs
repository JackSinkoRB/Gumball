using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class StepOptionController : MonoBehaviour
    {

        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private SettingsStepOption[] stepOptions;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private int currentIndex = -1;

        public void Select(int index)
        {
            if (index > stepOptions.Length - 1 || index < 0)
            {
                Debug.LogError($"Tried selecting index out of bounds for the options ({gameObject.name})");
                return;
            }

            if (index == currentIndex)
                return; //already selected
            
            if (currentIndex >= 0)
                stepOptions[currentIndex].OnDeselected();
            
            currentIndex = index;
            stepOptions[index].OnSelected();

            label.text = stepOptions[currentIndex].Label;

            nextButton.interactable = index + 1 <= stepOptions.Length - 1;
            previousButton.interactable = index - 1 >= 0;
        } 

        public void SelectNext()
        {
            if (currentIndex >= stepOptions.Length - 1)
                return; //at end

            Select(currentIndex + 1);
        }

        public void SelectPrevious()
        {
            if (currentIndex <= 0)
                return; //at start

            Select(currentIndex - 1);
        }
        
    }
}
