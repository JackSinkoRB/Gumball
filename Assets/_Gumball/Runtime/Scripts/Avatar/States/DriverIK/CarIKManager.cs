using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Gumball
{
    /// <summary>
    /// Holds the desired positions for the avatar's body parts.
    /// </summary>
    public class CarIKManager : MonoBehaviour
    {

        //todo: button to spawn male drivers (and set up with the IK points)
        
        [Header("Driver")]
        [SerializeField] private IKPositionsInCar maleDriver;
        [SerializeField] private IKPositionsInCar femaleDriver;

        [Header("Passenger")]
        [SerializeField] private IKPositionsInCar malePassenger;
        [SerializeField] private IKPositionsInCar femalePassenger;

        public IKPositionsInCar MaleDriver => maleDriver;
        public IKPositionsInCar FemaleDriver => femaleDriver;
        public IKPositionsInCar MalePassenger => malePassenger;
        public IKPositionsInCar FemalePassenger => femalePassenger;

    }
}
