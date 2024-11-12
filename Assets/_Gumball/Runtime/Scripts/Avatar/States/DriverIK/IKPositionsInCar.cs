using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct IKPositionsInCar
    {
        [SerializeField] private Transform pelvis;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private Transform leftKnee;
        [SerializeField] private Transform leftFoot;
        [SerializeField] private Transform rightKnee;
        [SerializeField] private Transform rightFoot;

        public Transform Pelvis => pelvis;
        public Transform LeftHand => leftHand;
        public Transform RightHand => rightHand;
        public Transform LeftKnee => leftKnee;
        public Transform LeftFoot => leftFoot;
        public Transform RightKnee => rightKnee;
        public Transform RightFoot => rightFoot;
    }
}
