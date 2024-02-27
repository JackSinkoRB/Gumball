using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarDrivingState : DynamicState
    {
        
        private Avatar avatar;
        
        private Animator animator => avatar.CurrentBody.GetComponent<Animator>();
        private CarIKManager ikManager => WarehouseManager.Instance.CurrentCar.AvatarIKManager;
        private bool isDriver => AvatarManager.Instance.DriverAvatar == avatar;
        private bool isDriverMale => AvatarManager.Instance.DriverAvatar.CurrentBodyType == AvatarBodyType.MALE;
        private bool isPassengerMale => AvatarManager.Instance.CoDriverAvatar.CurrentBodyType == AvatarBodyType.MALE;
        private IKPositionsInCar currentPositions => isDriver
            ? (isDriverMale ? ikManager.MaleDriver : ikManager.FemaleDriver)
            : (isPassengerMale ? ikManager.MalePassenger : ikManager.FemalePassenger);
        
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
            float distanceToFeet = avatar.CurrentBody.Pelvis.position.y - avatar.CurrentBody.TransformBone.transform.position.y;
            Vector3 desiredPosition = currentPositions.Pelvis.position.OffsetY(-distanceToFeet);
            avatar.transform.position = desiredPosition;
            avatar.transform.localRotation = Quaternion.Euler(Vector3.zero);

            avatar.CurrentBody.TransformBone.enabled = false;
            avatar.CurrentBody.DriverIK.Initialise(currentPositions);
        }

        public override void OnEndState()
        {
            base.OnEndState();
            
            animator.enabled = true;
            avatar.CurrentBody.TransformBone.enabled = true;
        }
        
    }
}
