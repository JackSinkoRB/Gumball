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

        private Action onConfirm;
        private Action onCancel;

        public void Initialise(string title, string description, Action onConfirm, Action onCancel = null)
        {
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
            
            titleLabel.text = title;
            descriptionLabel.text = description;
        }

        public void OnClickConfirm()
        {
            onConfirm?.Invoke();
            Hide();
        }

        public void OnClickCancel()
        {
            onCancel?.Invoke();
            Hide();
        }
        
    }
}
