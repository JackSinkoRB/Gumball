using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DecalLayerSelector : MonoBehaviour
    {
        
        [SerializeField] private MagneticScroll magneticScroll;

        private void OnEnable()
        {
            DecalManager.Instance.onCreateLiveDecal += OnCreateLiveDecal;
            DecalManager.Instance.onDestroyLiveDecal += OnDestroyLiveDecal;
            Populate();
        }

        private void OnDisable()
        {
            DecalManager.Instance.onCreateLiveDecal -= OnCreateLiveDecal;
            DecalManager.Instance.onDestroyLiveDecal -= OnDestroyLiveDecal;
        }

        private void OnCreateLiveDecal()
        {
            Populate();
        }
        
        private void OnDestroyLiveDecal()
        {
            Populate();   
        }

        public void Populate()
        {
            if (DecalManager.Instance.LiveDecals.Count == 0)
                return;
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            foreach (LiveDecal liveDecal in DecalManager.Instance.LiveDecals)
            {
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () => scrollItem.CurrentIcon.ImageComponent.sprite = liveDecal.Sprite;

                scrollItems.Add(scrollItem);
            }

            magneticScroll.SetItems(scrollItems);
        }
        
    }
}
