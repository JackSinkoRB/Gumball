using System;
using System.Collections;
using System.Collections.Generic;
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
            DecalManager.Instance.onCreateLiveDecal += OnCreateLiveDecal;
            DecalManager.Instance.onDestroyLiveDecal += OnDestroyLiveDecal;
            DecalManager.Instance.onSelectLiveDecal += OnSelectLiveDecal;
            DecalManager.Instance.onDeselectLiveDecal += OnDeselectLiveDecal;
            UpdateLayers();
        }

        private void OnDisable()
        {
            if (DecalManager.ExistsRuntime)
            {
                DecalManager.Instance.onCreateLiveDecal -= OnCreateLiveDecal;
                DecalManager.Instance.onDestroyLiveDecal -= OnDestroyLiveDecal;
                DecalManager.Instance.onSelectLiveDecal -= OnSelectLiveDecal;
                DecalManager.Instance.onDeselectLiveDecal -= OnDeselectLiveDecal;
            }
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
            magneticScroll.SnapItemToMagnet(DecalManager.Instance.GetPriorityOfLiveDecal(liveDecal));
        }

        private void OnDeselectLiveDecal(LiveDecal liveDecal)
        {
            
        }

        public void UpdateLayers()
        {
            layerAmountLabel.text = $"{DecalManager.Instance.LiveDecals.Count} / {DecalManager.MaxDecalsAllowed}";
            
            PopulateScroll();
        }

        private void PopulateScroll()
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            for (int index = 0; index < DecalManager.Instance.LiveDecals.Count; index++)
            {
                LiveDecal liveDecal = DecalManager.Instance.LiveDecals[index];
                int finalIndex = index;
   
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () =>
                {
                    DecalLayerIcon decalLayerIcon = (DecalLayerIcon) scrollItem.CurrentIcon;
                    decalLayerIcon.ImageComponent.sprite = liveDecal.Sprite;
                    decalLayerIcon.PriorityLabel.text = finalIndex.ToString();
                };
                
                //onSelectComplete gets called when the pointer is no longer down
                scrollItem.onSelectComplete += () =>
                {
                    if (DecalManager.Instance.CurrentSelected != liveDecal)
                        DecalManager.Instance.SelectLiveDecal(liveDecal);
                };

                scrollItems.Add(scrollItem);
            }

            magneticScroll.SetItems(scrollItems);
        }
        
    }
}
