using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class GameSessionNode : MonoBehaviour
    {

        [SerializeField] private GameSession session;
        [SerializeField] private TextMeshProUGUI typeLabel;

        public GameSession Session => session;
        
        private void OnEnable()
        {
            typeLabel.text = session.Type;
        }

        public void OnClicked()
        {
            PanelManager.GetPanel<GameSessionNodePanel>().Show();
            PanelManager.GetPanel<GameSessionNodePanel>().Initialise(this);
        }

    }
}
