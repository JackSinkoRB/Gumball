using System;
using System.Collections;
using System.Collections.Generic;
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
        }

        private void OnDeselectLiveDecal(LiveDecal liveDecal)
        {
            
        }

        public void UpdateLayers()
        {
            layerAmountLabel.text = $"{DecalEditor.Instance.LiveDecals.Count} / {DecalEditor.MaxDecalsAllowed}";
            
            PopulateScroll();
        }

        private void PopulateScroll()
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            for (int index = 0; index < DecalEditor.Instance.LiveDecals.Count; index++)
            {
                LiveDecal liveDecal = DecalEditor.Instance.LiveDecals[index];
                int finalIndex = index;
   
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () =>
                {
                    DecalLayerIcon decalLayerIcon = (DecalLayerIcon) scrollItem.CurrentIcon;
                    decalLayerIcon.ImageComponent.sprite = liveDecal.Sprite;
                    decalLayerIcon.PriorityLabel.text = (finalIndex + 1).ToString();
                };
                
                //onSelectComplete gets called when the pointer is no longer down
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
