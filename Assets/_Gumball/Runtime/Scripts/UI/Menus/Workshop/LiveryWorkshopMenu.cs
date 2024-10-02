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
        
    }
}
