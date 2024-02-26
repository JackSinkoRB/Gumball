using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// Holds the desired positions for the avatar's body parts.
    /// </summary>
    public class CarIKPositions : MonoBehaviour
    {
        
        [Header("Driving")]
        [SerializeField] private Transform pelvisDriving;
        [SerializeField] private Transform leftHandPositionDriving;
        [SerializeField] private Transform rightHandPositionDriving;
        [SerializeField] private Transform leftFootPositionDriving;
        [SerializeField] private Transform rightFootPositionDriving;
        
        [Header("Passenger")]
        [SerializeField] private Transform pelvisPassenger;
        [SerializeField] private Transform leftHandPositionPassenger;
        [SerializeField] private Transform rightHandPositionPassenger;
        [SerializeField] private Transform leftFootPositionPassenger;
        [SerializeField] private Transform rightFootPositionPassenger;

        public Transform LeftHandPositionDriving => leftHandPositionDriving;
        public Transform RightHandPositionDriving => rightHandPositionDriving;
        public Transform LeftHandPositionPassenger => leftHandPositionPassenger;
        public Transform RightHandPositionPassenger => rightHandPositionPassenger;
        public Transform LeftFootPositionDriving => leftFootPositionDriving;
        public Transform RightFootPositionDriving => rightFootPositionDriving;
        public Transform LeftFootPositionPassenger => leftFootPositionPassenger;
        public Transform RightFootPositionPassenger => rightFootPositionPassenger;
        public Transform PelvisDriving => pelvisDriving;
        public Transform PelvisPassenger => pelvisPassenger;

    }
}
