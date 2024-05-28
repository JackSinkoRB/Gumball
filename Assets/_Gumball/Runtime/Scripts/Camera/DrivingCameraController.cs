using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DrivingCameraController : CameraController
    {
        
        [Header("States")]
        [SerializeField] private DrivingCameraState[] drivingStates;
        [SerializeField] private DrivingCameraState rearViewMirrorState;
        [SerializeField] private DrivingCameraState introState;
        [SerializeField] private DrivingCameraState outroState;

        public DrivingCameraState RearViewMirrorState => rearViewMirrorState;
        public DrivingCameraState IntroState => introState;
        public DrivingCameraState OutroState => outroState;

        public DrivingCameraState CurrentDrivingState
        {
            get => drivingStates[DataManager.Settings.Get($"Camera.CurrentDrivingState", 0)];
            set => DataManager.Settings.Set($"Camera.CurrentDrivingState", drivingStates.IndexOfItem(value));
        }

        /// <summary>
        /// Switches the camera state to the next driving state. 
        /// </summary>
        public void SetNextDrivingState()
        {
            int currentIndex = drivingStates.IndexOfItem(CurrentDrivingState);
            int nextIndex = currentIndex + 1;

            if (nextIndex >= drivingStates.Length)
                nextIndex = 0;
            
            DrivingCameraState nextDrivingState = drivingStates[nextIndex];
            
            CurrentDrivingState = nextDrivingState;
            SetState(nextDrivingState);
            SkipTransition();
        }
        
    }
}
