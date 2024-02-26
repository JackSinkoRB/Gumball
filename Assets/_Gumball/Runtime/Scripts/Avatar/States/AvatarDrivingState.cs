using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class AvatarDrivingState : DynamicState
    {
        
        private Avatar avatar;
        
        private Animator animator => avatar.CurrentBody.GetComponent<Animator>();
        private CarIKPositions ikPositions => WarehouseManager.Instance.CurrentCar.AvatarIKPositions;
        private bool isDriver => AvatarManager.Instance.DriverAvatar == avatar; 

        public override void SetUp(DynamicStateManager manager)
        {
            base.SetUp(manager);

            avatar = transform.GetComponentInAllParents<Avatar>();
        }

        public override void OnSetCurrent()
        {
            base.OnSetCurrent();

            animator.enabled = false;

            //set child of car
            avatar.transform.SetParent(WarehouseManager.Instance.CurrentCar.transform);
            
            //teleport the avatar to the desired pelvis position
            Transform seatedPosition = isDriver ? ikPositions.PelvisDriving : ikPositions.PelvisPassenger;
            avatar.transform.position = seatedPosition.position;
            avatar.transform.localRotation = Quaternion.Euler(Vector3.zero);

            avatar.CurrentBody.DriverIK.Initialise(isDriver, WarehouseManager.Instance.CurrentCar.AvatarIKPositions);
        }

        public override void OnEndState()
        {
            base.OnEndState();
            
            animator.enabled = true;
        }
        
    }
}
