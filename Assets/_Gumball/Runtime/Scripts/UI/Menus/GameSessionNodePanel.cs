using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class GameSessionNodePanel : AnimatedPanel
    {

        private GameSessionNode currentNode;
        
        public void Initialise(GameSessionNode node)
        {
            currentNode = node;
        }
        
        protected override void OnHide()
        {
            base.OnHide();

            if (MapSceneManager.ExistsRuntime)
                MapSceneManager.Instance.RemoveFocusOnNode();
        }

        public void OnClickPlayButton()
        {
            if (!FuelManager.HasFuel())
            {
                Hide();
                PanelManager.GetPanel<InsufficientFuelPanel>().Show();
                return;
            }
            
            currentNode.GameSession.StartSession();
        }
        
    }
}