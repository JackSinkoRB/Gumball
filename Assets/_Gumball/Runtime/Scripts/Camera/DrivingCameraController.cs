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
        [SerializeField] private CameraState[] drivingStates;
        [SerializeField] private CameraState rearViewMirrorState;
        [SerializeField] private CameraState introState;
        [SerializeField] private CameraState outroState;

        public CameraState RearViewMirrorState => rearViewMirrorState;
        public CameraState IntroState => introState;
        public CameraState OutroState => outroState;

        public CameraState CurrentDrivingState
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
            
            CameraState nextDrivingState = drivingStates[nextIndex];
            
            CurrentDrivingState = nextDrivingState;
            SetState(nextDrivingState);
        }
        
    }
}
