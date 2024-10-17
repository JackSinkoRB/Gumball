using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapSubPartPanel : AnimatedPanel
    {

        [SerializeField] private SwapSubPartInstallButton installButton;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private Image icon;
        [SerializeField] private PerformanceRatingSlider maxSpeedSlider;
        [SerializeField] private PerformanceRatingSlider accelerationSlider;
        [SerializeField] private PerformanceRatingSlider handlingSlider;
        [SerializeField] private PerformanceRatingSlider nosSlider;

        [Header("Events")]
        [SerializeField] private Transform eventHolder;
        [SerializeField] private EventScrollItem eventPrefab;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private SubPartSlot slot;

        private AICar currentCar => WarehouseManager.Instance.CurrentCar;
        
        public void Initialise(SubPartSlot slot)
        {
            this.slot = slot;

            UpdateTitle();
            UpdateIcon();
            UpdateDescription();
            UpdatePerformanceRatingSliders();
            UpdateEvents();
            installButton.Initialise(slot);
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            PanelManager.GetPanel<PaintStripeBackgroundPanel>().Show();
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            if (PanelManager.PanelExists<PaintStripeBackgroundPanel>())
                PanelManager.GetPanel<PaintStripeBackgroundPanel>().Hide();
        }
        
        public void UpdatePerformanceRatingSliders()
        {
            CarPerformanceProfile profileWithPart;
            if (slot.CurrentSubPart == null)
            {
                //figure out what the performance ratings would be if it was applied and then undo it
                
                //install the part on the car to get the performance profile
                SubPart sparePart = SubPartManager.GetSpareSubPart(slot.Type, slot.Rarity);
                if (sparePart == null)
                    sparePart = SubPartManager.GetSubParts(slot.Type, slot.Rarity).First();
                if (sparePart == null)
                    throw new NullReferenceException($"Could not update performance ratings as there's no sub parts of type {slot.Type} with rarity {slot.Rarity}");
                
                slot.InstallSubPart(sparePart);

                //create a profile with the specific core part with the sub part installed
                profileWithPart = new CarPerformanceProfile(currentCar.CarIndex);

                //uninstall
                slot.UninstallSubPart();
            }
            else
            {
                //is already applied, use the current profile
                profileWithPart = new CarPerformanceProfile(currentCar.CarIndex);
            }
            
            CarPerformanceProfile currentProfile = new CarPerformanceProfile(currentCar.CarIndex);
            maxSpeedSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
            accelerationSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
            handlingSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
            nosSlider.Initialise(currentCar.PerformanceSettings, currentProfile, profileWithPart);
        }

        private void UpdateIcon()
        {
            icon.sprite = slot.Icon;
        }
        
        private void UpdateTitle()
        {
            titleLabel.text = $"{slot.DisplayName}";
        }
        
        private void UpdateDescription()
        {
            descriptionLabel.text = $"{slot.Description}";
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
