using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapSubPartPanel : AnimatedPanel
    {

        [SerializeField] private SwapSubPartInstallButton installButton;

        [Header("Events")]
        [SerializeField] private Transform eventHolder;
        [SerializeField] private EventScrollItem eventPrefab;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private SubPartSlot slot;

        public void Initialise(SubPartSlot slot)
        {
            this.slot = slot;

            UpdateEvents();
            installButton.Initialise(slot);
        }

        private void UpdateEvents()
        {
            //for all parts that match the slot, show all the game sessions that reward it
            foreach (SubPart matchingPart in SubPartManager.GetSubParts(slot.Type, slot.Rarity))
            {
                foreach (GameSession session in matchingPart.SessionsThatGiveReward)
                {
                    EventScrollItem eventScrollItem = eventPrefab.gameObject.GetSpareOrCreate<EventScrollItem>(eventHolder);
                    eventScrollItem.transform.SetAsLastSibling();
                    eventScrollItem.Initialise(session, matchingPart);
                }
            }
        }

    }
}
