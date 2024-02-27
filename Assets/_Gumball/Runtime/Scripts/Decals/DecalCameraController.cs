using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DecalCameraController : RotatingCameraController
    {
        
        [Header("Decals")]
        [SerializeField] private Vector3 decalTargetOffset = new(0, -1f);
        
        private int firstFrame;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            firstFrame = Time.frameCount;
            
            DecalEditor.onSelectLiveDecal += OnSelectDecal;
            DecalEditor.onDeselectLiveDecal += OnDeselectDecal;

            SetTarget(WarehouseManager.Instance.CurrentCar.transform, defaultTargetOffset);
            SetInitialPosition();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            DecalEditor.onSelectLiveDecal -= OnSelectDecal;
            DecalEditor.onDeselectLiveDecal -= OnDeselectDecal;
        }

        private void OnSelectDecal(LiveDecal selectedDecal)
        {
            SetTarget(selectedDecal.transform, decalTargetOffset);
        }
        
        private void OnDeselectDecal(LiveDecal decalDeselected)
        {
            bool isFirstFrame = firstFrame == Time.frameCount;
            if (!isFirstFrame)
            {
                //don't set the camera target if it's the first frame, as the initial position was set
                SetTarget(DecalEditor.Instance.CurrentCar.transform, defaultTargetOffset);
            }
        }
        
    }
}
