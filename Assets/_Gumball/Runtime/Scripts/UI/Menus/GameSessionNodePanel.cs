using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class GameSessionNodePanel : GameSessionInfoPanel
    {
        
        [Header("Node panel")]
        [SerializeField] private TextMeshProUGUI modeLabel;

        public override void Initialise(GameSession session)
        {
            base.Initialise(session);
            
            modeLabel.text = session.GetModeDisplayName();
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (MapSceneManager.ExistsRuntime)
                MapSceneManager.Instance.RemoveFocusOnNode();
            
            if (PanelManager.PanelExists<GameSessionMapPanel>())
                PanelManager.GetPanel<GameSessionMapPanel>().Show();
        }

        public void OnClickPlayButton()
        {
            if (!FuelManager.Instance.HasFuel())
            {
                Hide();
                PanelManager.GetPanel<InsufficientFuelPanel>().Show();
                return;
            }
            
            session.StartSession();
        }

    }
}