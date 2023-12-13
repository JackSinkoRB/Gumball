using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class DecalLayerSelector : MonoBehaviour
    {
        
        [SerializeField] private MagneticScroll magneticScroll;
        [SerializeField] private TextMeshProUGUI layerAmountLabel;

        public MagneticScroll MagneticScroll => magneticScroll;
        
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
        
        public void UpdateLayers()
        {
            layerAmountLabel.text = $"{DecalEditor.Instance.LiveDecals.Count} / {DecalEditor.MaxDecalsAllowed}";
            
            PopulateScroll();
        }
        
        public void SnapToLiveDecal(LiveDecal liveDecal)
        {
            magneticScroll.SnapItemToMagnet(liveDecal.Priority - 1);
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
            SnapToLiveDecal(liveDecal);
            
            liveDecal.onColorChanged += OnSelectedColourChanged;
        }

        private void OnDeselectLiveDecal(LiveDecal liveDecal)
        {
            liveDecal.onColorChanged -= OnSelectedColourChanged;
        }

        private void OnSelectedColourChanged(Color oldColor, Color newColor)
        {
            magneticScroll.Items[DecalEditor.Instance.CurrentSelected.Priority - 1].CurrentIcon.ImageComponent.color = newColor;
        }

        public void PopulateScroll()
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            foreach (LiveDecal liveDecal in DecalEditor.Instance.LiveDecals)
            {
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () =>
                {
                    DecalLayerIcon decalLayerIcon = (DecalLayerIcon) scrollItem.CurrentIcon;
                    decalLayerIcon.ImageComponent.sprite = liveDecal.Sprite;
                    decalLayerIcon.ImageComponent.color = liveDecal.Color;
                    decalLayerIcon.PriorityLabel.text = liveDecal.Priority.ToString();
                };
                
                scrollItem.onSelectComplete += () =>
                {
                    DecalEditor.Instance.SelectLiveDecal(liveDecal);
                };

                scrollItems.Add(scrollItem);
            }
            
            magneticScroll.SetItems(scrollItems, magneticScroll.LastSelectedItemIndex);
        }
        
    }
}
