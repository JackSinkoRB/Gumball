using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gumball
{
    public class PartDetails : MonoBehaviour
    {
        //Attached to each car
        public PartType type;
        public string partName;
        [TextArea(3, 10)] public string PartDescription;
        public Sprite PartImage;
        public int Price; //Buy price
        public int UnlockLevel;
        public bool Premium;

        public PartCategory EnginePartCategory;
        public bool EnginePartSupportsBlower;
        public bool EnginePartSupportsTurbo;
        public bool EnginePartSupportsNA;

        public float EnginePartPower = 0;
        public PartPowerType enginePowerType = PartPowerType.Static;
        public GameObject linkedMesh;

        public string uniqueIdentifier;
        public bool isActivePart;


#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!Application.isPlaying && uniqueIdentifier == null ||
                !Application.isPlaying && uniqueIdentifier.Length < 8)
            {
                GenerateID();
            }
        }

        private void GenerateID()
        {
            if (EnginePartCategory == PartCategory.cosmetic)
            {
                uniqueIdentifier = $"{type}{transform.name}#{transform.GetSiblingIndex()}";
            }
            else if (GetComponentInParent<EngineCustomisation>() != null)
            {
                uniqueIdentifier =
                    $"{type}{transform.GetSiblingIndex()}{GetComponentInParent<EngineCustomisation>().engineName}";
            }
            else
            {
                uniqueIdentifier = "!NULL!";
            }
        }

#endif
    }

    public enum PartPowerType
    {
        Static,
        percentage
    }

    public enum PartType
    {
        none,
        frontBars,
        rearBars,
        skirts,
        guards,
        bonnets,
        spoilers,

        intakeManifold,
        exhaustManifold,
        cylinderHead,
        fuelSystem,
        rotatingAssembly,
        camshaft,
        inductionType,
        wheel


        /*
         * switch (targetType)
                {
                    case PartType.frontBars:
                        return currentVehicleData.frontBars;
                    case PartType.rearBars:
                        return currentVehicleData.rearBars;
                    case PartType.bonnets:
                        return currentVehicleData.bonnets;
                    case PartType.gaurds:
                        return currentVehicleData.gaurds;
                    case PartType.skirts:
                        return currentVehicleData.sideSkirts;
                    case PartType.intakeManifold:
                        return currentVehicleData.intakeManifold;
                    case PartType.exhaustManifold:
                        return currentVehicleData.exhaustHeader;
                    case PartType.cylinderHead:
                        return currentVehicleData.cylinderHead;
                    case PartType.fuelSystem:
                        return currentVehicleData.fuelSystem;
                    case PartType.rotatingAssembly:
                        return currentVehicleData.rotatingAssembly;
                    case PartType.camshaft:
                        return currentVehicleData.camshaft;
                }
         * */
    }


    public enum PartCategory
    {
        cosmetic,
        engine
    }
}