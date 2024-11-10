using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class ConfirmationPanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private Transform costHolder;
        [SerializeField] private AutosizeTextMeshPro costLabel;

        private Action onConfirm;
        private Action onCancel;

        private int standardCurrencyCost; 

        public void Initialise(string title, string description, Action onConfirm, Action onCancel = null, int standardCurrencyCost = 0)
        {
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
            this.standardCurrencyCost = standardCurrencyCost;
            
            titleLabel.text = title;
            descriptionLabel.text = description;
            
            costHolder.gameObject.SetActive(standardCurrencyCost > 0);
            costLabel.text = standardCurrencyCost.ToString();
            costLabel.Resize();
        }

        public void OnClickConfirm()
        {
            onConfirm?.Invoke();
            Hide();

            //check to take funds
            if (standardCurrencyCost > 0)
            {
                if (!Currency.Standard.HasEnoughFunds(standardCurrencyCost))
                {
                    Hide();
                    PanelManager.GetPanel<InsufficientStandardCurrencyPanel>().Show();
                    return;
                }
                
                Currency.Standard.TakeFunds(standardCurrencyCost);
            }
        }

        public void OnClickCancel()
        {
            onCancel?.Invoke();
            Hide();
        }
        
    }
}
