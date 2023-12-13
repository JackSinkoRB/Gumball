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
            DecalEditor.Instance.onCreateLiveDecal += OnCreateLiveDecal;
            DecalEditor.Instance.onDestroyLiveDecal += OnDestroyLiveDecal;
            DecalEditor.Instance.onSelectLiveDecal += OnSelectLiveDecal;
            DecalEditor.Instance.onDeselectLiveDecal += OnDeselectLiveDecal;
            
            DecalEditor.onSessionStart += OnStartSession;
        }

        private void OnDisable()
        {
            if (DecalEditor.ExistsRuntime)
            {
                DecalEditor.Instance.onCreateLiveDecal -= OnCreateLiveDecal;
                DecalEditor.Instance.onDestroyLiveDecal -= OnDestroyLiveDecal;
                DecalEditor.Instance.onSelectLiveDecal -= OnSelectLiveDecal;
                DecalEditor.Instance.onDeselectLiveDecal -= OnDeselectLiveDecal;
                
                DecalEditor.onSessionStart -= OnStartSession;
            }
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
            magneticScroll.SnapItemToMagnet(DecalEditor.Instance.GetPriorityOfLiveDecal(liveDecal));
            
            liveDecal.onColorChanged += OnSelectedColourChanged;
        }

        private void OnDeselectLiveDecal(LiveDecal liveDecal)
        {
            liveDecal.onColorChanged -= OnSelectedColourChanged;
        }

        private void OnSelectedColourChanged(Color oldColor, Color newColor)
        {
            magneticScroll.Items[DecalEditor.Instance.CurrentSelected.Priority].CurrentIcon.ImageComponent.color = newColor;
        }

        public void UpdateLayers()
        {
            layerAmountLabel.text = $"{DecalEditor.Instance.LiveDecals.Count} / {DecalEditor.MaxDecalsAllowed}";
            
            PopulateScroll();
        }

        private void PopulateScroll()
        {
            List<LiveDecal> decalsSorted = DecalEditor.Instance.LiveDecals.OrderBy(liveDecal => liveDecal.Priority).ToList();
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            foreach (LiveDecal liveDecal in decalsSorted)
            {
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () =>
                {
                    DecalLayerIcon decalLayerIcon = (DecalLayerIcon) scrollItem.CurrentIcon;
                    decalLayerIcon.ImageComponent.sprite = liveDecal.Sprite;
                    decalLayerIcon.ImageComponent.color = liveDecal.Color;
                    decalLayerIcon.PriorityLabel.text = (liveDecal.Priority + 1).ToString();
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
