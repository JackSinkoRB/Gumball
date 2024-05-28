using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CockpitCameraState : DrivingCameraState
    {

        private const float nearClipPlane = 0.1f;

        private float initialNearClipPlane;
        
        public override void OnSetCurrent(CameraController controller)
        {
            base.OnSetCurrent(controller);
            
            if (WarehouseManager.Instance.CurrentCar.RearViewCameraTarget == null)
                Debug.LogError($"{WarehouseManager.Instance.CurrentCar.name} is missing the cockpit camera target.");
            
            otherTarget = WarehouseManager.Instance.CurrentCar.CockpitCameraTarget;

            //setup interior view
            initialNearClipPlane = Camera.main.nearClipPlane;
            Camera.main.nearClipPlane = nearClipPlane;
            
            //disable driver avatar head
            AvatarManager.Instance.DriverAvatar.EnableHead(false);
        }
        
        public override void OnNoLongerCurrent()
        {
            base.OnNoLongerCurrent();

            Camera.main.nearClipPlane = initialNearClipPlane;
            
            AvatarManager.Instance.DriverAvatar.EnableHead(true);
        }
        
    }
}
