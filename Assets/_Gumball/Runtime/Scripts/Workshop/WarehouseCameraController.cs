using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WarehouseCameraController : RotatingCameraController
    {

        [Header("Decals")]
        [SerializeField] private Vector3 decalTargetOffset;

        private int firstFrame;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            WarehouseManager.Instance.onCurrentCarChanged += OnCurrentCarChanged;
            
            firstFrame = Time.frameCount;
            
            DecalEditor.onSelectLiveDecal += OnSelectDecal;
            DecalEditor.onDeselectLiveDecal += OnDeselectDecal;

            if (WarehouseManager.Instance.CurrentCar != null)
                SetTarget(WarehouseManager.Instance.CurrentCar.transform, defaultTargetOffset);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            WarehouseManager.Instance.onCurrentCarChanged -= OnCurrentCarChanged;
            
                        
            DecalEditor.onSelectLiveDecal -= OnSelectDecal;
            DecalEditor.onDeselectLiveDecal -= OnDeselectDecal;
        }

        private void OnCurrentCarChanged(AICar newcar)
        {
            SetTarget(newcar.transform, DefaultTargetOffset);
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
