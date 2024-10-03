using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class LiveryWorkshopMenu : WorkshopSubMenu
    {

        [Header("Layer selector")]
        [SerializeField] private TextMeshProUGUI layerAmountLabel;
        [SerializeField] private DecalLayerOption layerOptionPrefab;
        [SerializeField] private Transform layerOptionHolder;
        
        [Header("Buttons")]
        [SerializeField] private MultiImageButton trashButton;
        [SerializeField] private MultiImageButton colourButton;
        [SerializeField] private MultiImageButton sendForwardButton;
        [SerializeField] private MultiImageButton sendBackwardButton;
        [SerializeField] private MultiImageButton undoButton;
        [SerializeField] private MultiImageButton redoButton;
        
        public MultiImageButton TrashButton => trashButton;
        public MultiImageButton ColourButton => colourButton;
        public MultiImageButton SendForwardButton => sendForwardButton;
        public MultiImageButton SendBackwardButton => sendBackwardButton;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            DecalEditor.Instance.StartSession(WarehouseManager.Instance.CurrentCar);
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            CoroutineHelper.Instance.StartCoroutine(DecalEditor.Instance.EndSession());
        }

        private void OnEnable()
        {
            DecalEditor.onCreateLiveDecal += OnCreateLiveDecal;
            DecalEditor.onDestroyLiveDecal += OnDestroyLiveDecal;
            DecalEditor.onSelectLiveDecal += OnSelectLiveDecal;
            DecalEditor.onDeselectLiveDecal += OnDeselectLiveDecal;
            
            DecalEditor.onSessionStart += OnStartSession;
        }

        private void OnDisable()
        {
            DecalEditor.onCreateLiveDecal -= OnCreateLiveDecal;
            DecalEditor.onDestroyLiveDecal -= OnDestroyLiveDecal;
            DecalEditor.onSelectLiveDecal -= OnSelectLiveDecal;
            DecalEditor.onDeselectLiveDecal -= OnDeselectLiveDecal;
                
            DecalEditor.onSessionStart -= OnStartSession;
        }
        
        private void OnStartSession()
        {
            UpdateLayers();
        }

        private void OnCreateLiveDecal(LiveDecal liveDecal)
        {
            UpdateLayers();
        }
        
        private void OnDestroyLiveDecal(LiveDecal liveDecal)
        {
            UpdateLayers();   
        }
        
        private void OnSelectLiveDecal(LiveDecal liveDecal)
        {
            //TODO:
            //SnapToLiveDecal(liveDecal);
            
            liveDecal.onColorChanged += OnSelectedDecalColourChanged;
        }

        private void OnDeselectLiveDecal(LiveDecal liveDecal)
        {
            liveDecal.onColorChanged -= OnSelectedDecalColourChanged;
        }

        private void OnSelectedDecalColourChanged(Color oldColor, Color newColor)
        {
            //update the color of the layer option
            layerOptionHolder.GetChild(DecalEditor.Instance.CurrentSelected.Priority - 1).GetComponent<DecalLayerOption>().TextureIcon.color = newColor;
        }
        
        private void UpdateLayers()
        {
            layerAmountLabel.text = $"{DecalEditor.Instance.LiveDecals.Count}/{DecalEditor.MaxDecalsAllowed}";
            
            //populate the layers
            foreach (Transform child in layerOptionHolder)
                child.gameObject.Pool();

            foreach (LiveDecal liveDecal in DecalEditor.Instance.LiveDecals)
            {
                DecalLayerOption instance = layerOptionPrefab.gameObject.GetSpareOrCreate<DecalLayerOption>(layerOptionHolder);
                instance.Initialise(liveDecal);
            }
        }
        
        public void OnClickTrashButton()
        {
            DecalStateManager.LogStateChange(new DecalStateManager.DestroyStateChange(DecalEditor.Instance.CurrentSelected));
            DecalEditor.Instance.DisableLiveDecal(DecalEditor.Instance.CurrentSelected);
        }

        public void OnClickSendForwardButton()
        {
            SendBackwardOrForward(true, DecalEditor.Instance.CurrentSelected);
        }
        
        public void OnClickSendBackwardButton()
        {
            SendBackwardOrForward(false, DecalEditor.Instance.CurrentSelected);
        }
        
        public void OnClickColourButton()
        {
            //TODO:
            // if (colourPickerEnabled)
            //     DisableColourPicker();
            // else
            // {
            //     EnableColourPicker();
            //     ColourSelectorPanel.Populate(DecalEditor.Instance.CurrentSelected);
            // }
        }
        
        public void OnClickUndoButton()
        {
            DecalStateManager.UndoLatestChange();
        }

        public void OnClickRedoButton()
        {
            DecalStateManager.RedoLatestUndo();
        }
        
        private void SendBackwardOrForward(bool isForward, LiveDecal liveDecal)
        {
            List<LiveDecal> overlappingDecals = liveDecal.GetOverlappingLiveDecals();
                
            liveDecal.SendBackwardOrForward(isForward, overlappingDecals);
   
            UpdateSendForwardBackwardButtons(liveDecal, overlappingDecals);

            //TODO:
            //layerSelector.PopulateScroll(); //order has changed, so need to repopulate
            //layerSelector.SnapToLiveDecal(liveDecal);
        }
        
        private void UpdateSendForwardBackwardButtons(LiveDecal liveDecal, List<LiveDecal> overlappingDecals = null)
        {
            bool hasDecalWithHigherPriority = false;
            bool hasDecalWithLowerPriority = false;

            overlappingDecals ??= liveDecal.GetOverlappingLiveDecals();
            
            foreach (LiveDecal decal in overlappingDecals)
            {
                if (hasDecalWithHigherPriority && hasDecalWithLowerPriority)
                    break; //no need to continue
                
                if (decal.Priority > liveDecal.Priority)
                {
                    hasDecalWithHigherPriority = true;
                    continue;
                }
                
                if (decal.Priority < liveDecal.Priority)
                {
                    hasDecalWithLowerPriority = true;
                    continue;
                }
            }

            sendForwardButton.interactable = hasDecalWithHigherPriority;
            sendBackwardButton.interactable = hasDecalWithLowerPriority;
        }
        
    }
}
