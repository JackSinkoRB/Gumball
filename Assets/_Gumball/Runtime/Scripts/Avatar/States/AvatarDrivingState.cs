using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarDrivingState : AvatarDynamicState
    {
        
        private CarIKManager ikManager => carBelongsTo.AvatarIKManager;
        private IKPositionsInCar currentPositions => isDriver ? ikManager.Driver : ikManager.Passenger;

        private bool isDriverCached;
        private bool isDriver
        {
            get
            {
                if (!Application.isPlaying && carBelongsToCached != null)
                    return isDriverCached;
                
                return AvatarManager.Instance.DriverAvatar == avatar;
            }
        }
        
        private AICar carBelongsToCached;
        private AICar carBelongsTo
        {
            get
            {
                if (!Application.isPlaying && carBelongsToCached != null)
                    return carBelongsToCached;

                return WarehouseManager.Instance.CurrentCar;
            }
        }

        public override void OnSetCurrent()
        {
            base.OnSetCurrent();
            
            //set child of car
            avatar.transform.SetParent(carBelongsTo.transform);
            
            //teleport the avatar to the desired pelvis position
            float distanceToFeet = avatar.CurrentBody.Pelvis.position.y - avatar.CurrentBody.TransformBone.transform.position.y;
            Vector3 desiredPosition = currentPositions.Pelvis.position.OffsetY(-distanceToFeet);
            avatar.transform.position = desiredPosition;
            avatar.transform.localRotation = Quaternion.Euler(Vector3.zero);
            
            avatar.CurrentBody.TransformBone.enabled = false;
            avatar.CurrentBody.DriverIK.Initialise(currentPositions);

            if (isDriver && Application.isPlaying)
            {
                //set the steering wheel as the hand target
                currentPositions.LeftHand.SetParent(carBelongsTo.SteeringWheel.transform);
                currentPositions.RightHand.SetParent(carBelongsTo.SteeringWheel.transform);
            }
        }

        public override void OnEndState()
        {
            base.OnEndState();

            avatar.CurrentBody.DriverIK.enabled = false;
        }
        
#if UNITY_EDITOR
        public void SetupEditMode(AICar car, bool isDriver)
        {
            carBelongsToCached = car;
            isDriverCached = isDriver;
        }
#endif
        
    }
}
