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
            
            //TODO: unit tests - test adding fuel, make sure session can't be started if not enough fuel
            if (!FuelManager.HasFuel())
            {
                PanelManager.GetPanel<InsufficientFuelPanel>().Show();
                return;
            }

            currentNode.GameSession.StartSession();
        }
        
    }
}