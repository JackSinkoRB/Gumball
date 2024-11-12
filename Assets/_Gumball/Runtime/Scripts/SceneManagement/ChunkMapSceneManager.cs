using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Gumball
{
    public class ChunkMapSceneManager : Singleton<ChunkMapSceneManager>
    {

        [SerializeField] private DrivingCameraController drivingCameraController;

        public DrivingCameraController DrivingCameraController => drivingCameraController;

        private void OnDisable()
        {
            //set idle states
            if (AvatarManager.Instance.DriverAvatar != null)
                AvatarManager.Instance.DriverAvatar.StateManager.SetState<AvatarStandingIdleState>();
            if (AvatarManager.Instance.CoDriverAvatar != null)
                AvatarManager.Instance.CoDriverAvatar.StateManager.SetState<AvatarStandingIdleState>();
        }

    }
}
