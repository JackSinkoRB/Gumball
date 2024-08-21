using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class EventScrollItem : MonoBehaviour
    {

        private const string obtainedText = "Obtained!";
        
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI lockedLabel;

        private GameSession session;
        private SubPart subPart;
        private CorePart corePart;

        public void Initialise(GameSession session, SubPart part)
        {
            this.session = session;
            subPart = part;
            
            label.text = session.Description;
            
            //TODO: if session is not yet unlocked, set button disabled
            
            if (part.IsUnlocked)
            {
                button.interactable = false;
                lockedLabel.gameObject.SetActive(true);
                lockedLabel.text = obtainedText;
                
                //strikethrough the name
                label.fontStyle = FontStyles.Strikethrough;
            }
            else
            {
                button.interactable = true;
                lockedLabel.gameObject.SetActive(false);
                
                label.fontStyle = FontStyles.Normal;
            }
        }
        
        public void Initialise(GameSession session, CorePart part)
        {
            this.session = session;
            corePart = part;
            
            label.text = session.Description;
            
            //TODO: if session is not yet unlocked, set button disabled
            
            if (part.IsUnlocked)
            {
                button.interactable = false;
                lockedLabel.gameObject.SetActive(true);
                lockedLabel.text = obtainedText;
                
                //strikethrough the name
                label.fontStyle = FontStyles.Strikethrough;
            }
            else
            {
                button.interactable = true;
                lockedLabel.gameObject.SetActive(false);
                
                label.fontStyle = FontStyles.Normal;
            }
        }

        public void OnClick()
        {
            //show confirmation panel - if yes: load the game session
            PanelManager.GetPanel<ConfirmationPanel>().Show();
            PanelManager.GetPanel<ConfirmationPanel>().Initialise("Start session?", session.Description, () => session.StartSession());
        }

    }
}
