using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    [Serializable]
    public class CarData
    {

        public AssetReference assetReference;
        public string vehicleType = "";
        public string engineType = "";
        public int vehicleLevel = 0;
        public List<string> ownedParts = new();
        public int frontBars = 0;
        public int rearBars = 0;
        public int gaurds = 0;
        public int bonnets = 0;
        public int spoilers = 0;
        public int sideSkirts = 0;

        [Header("Engine customisation")] public int exhaustHeader = 0;
        public int intakeManifold = 0;

        public int cylinderHead = 0;
        public int fuelSystem = 0;
        public int rotatingAssembly = 0;
        public int camshaft = 0;

        [Tooltip("0 = default | 1 = NA | 2 = Turbo")]
        public int inductionType = 0;

        [Header("Wheel Customisation")]
        public string wheelType;
        public float wheelOffset = 0;
        public float wheelOffsetRear = 0;
        public float suspensionHeight = 0;
        public float wheelCamber = 0;
        public float wheelCamberRear = 0;
        public float stiffness = 0;
        public float wheelSize = 0;
        public float wheelSizeRear = 0;
        public float tireSize = 0;
        public float tireSizeRear = 0;
        public float tireWidth = 0;
        public float tireWidthRear = 0;
        public SerializableColor frontWheelColor = new(Color.grey);
        public SerializableColor frontWheelRim = new(Color.grey);
        public float frontWheelGloss = 0.75f;
        public SerializableColor rearWheelColor = new(Color.grey);
        public SerializableColor rearWheelRim = new(Color.grey);
        public float rearWheelGloss = 0.75f;
        public SerializableColor mainBodyColor = new(Color.white);
        public SerializableColor mainBodyInnerColor = new(Color.white);
        public SerializableColor mainBodyOuterColor = new(Color.white);
        public float mainBodyGloss = 0.95f;
        public float mainBodyMetallic = 0.1f;
    }
}