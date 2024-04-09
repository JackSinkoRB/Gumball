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
        [SerializeField] private CameraState drivingState;
        [SerializeField] private CameraState introState;
        [SerializeField] private CameraState outroState;

        public CameraState DrivingState => drivingState;
        public CameraState IntroState => introState;
        public CameraState OutroState => outroState;

    }
}
