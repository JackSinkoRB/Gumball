using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DialoguePanel : AnimatedPanel
    {

        [SerializeField] private Image characterIconImageLeft;
        [SerializeField] private Image characterIconImageRight;
        [SerializeField] private TextMeshProUGUI characterNameLabel;
        [SerializeField] private TextMeshProUGUI dialogueLabel;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Dialogue dialogue;
        
        public void Initialise(Dialogue dialogue)
        {
            this.dialogue = dialogue;
            
            dialogueLabel.text = dialogue.Message;
            characterNameLabel.text = dialogue.Character.DisplayName;

            characterIconImageLeft.sprite = dialogue.Character.Icon;
            characterIconImageRight.sprite = dialogue.Character.Icon;
            characterIconImageLeft.gameObject.SetActive(dialogue.CharacterPosition == Dialogue.ScreenPosition.LEFT);
            characterIconImageRight.gameObject.SetActive(dialogue.CharacterPosition == Dialogue.ScreenPosition.RIGHT);
        }

    }
}
