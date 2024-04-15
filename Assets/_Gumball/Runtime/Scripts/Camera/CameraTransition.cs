using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CameraTransition
    {
        
        [SerializeField] private CameraState from;
        [SerializeField] private CameraState to;

        [SerializeField] private float transitionTime = 1f;
        [SerializeField] private AnimationCurve blendCurve;

        public CameraState From => from;
        public CameraState To => to;
        public float TransitionTime => transitionTime;
        public AnimationCurve BlendCurve => blendCurve;
        
    }
}
