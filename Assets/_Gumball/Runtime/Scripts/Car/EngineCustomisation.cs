using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class EngineCustomisation : MonoBehaviour
    {
        [Header("Default details")] [Tooltip("If default is turbo then ensure NA has negative power")]
        public string engineName;

        public bool isDefaultTurbo;
        public bool isCurrentlyTurbo;
        public float defaultMaxTorque;
        public float totalTorque;
        public float minRpm = 800;
        public float maxRpm = 8000;
        public float torqueRpm = 4800;
        public float maxPower = 317000;
        public float powerRpm = 7000;
        public float targetFuel;
        public float targetAir;
        [Header("Unique engine parts")] public List<PartDetails> exhaustManifolds = new List<PartDetails>();
        public List<PartDetails> exhaustManifolds_Turbo = new List<PartDetails>();

        public List<PartDetails> intakeManifolds = new List<PartDetails>();
        public List<PartDetails> intakeManifolds_Turbo = new List<PartDetails>();

        [Header("Shared Parts")] public List<PartDetails> cylinderHeads = new List<PartDetails>();
        public List<PartDetails> fuelSystems = new List<PartDetails>();
        public List<PartDetails> rotatingAssemblys = new List<PartDetails>();
        public List<PartDetails> camshafts = new List<PartDetails>();
        public List<PartDetails> inductionTypes = new List<PartDetails>();

        private PartDetails[] allParts;
        private bool isSetup;

        void Setup()
        {
            allParts = GetComponentsInChildren<PartDetails>();

            for (int i = 0; i < allParts.Length; i++)
            {
                if (allParts[i].linkedMesh != null)
                {
                    //allParts[i].gameObject.SetActive(false);
                    allParts[i].linkedMesh.SetActive(false);
                }

                allParts[i].isActivePart = false;

                switch (allParts[i].type)
                {
                    case PartType.exhaustManifold:
                        if (allParts[i].EnginePartSupportsTurbo)
                        {
                            exhaustManifolds_Turbo.Add(allParts[i]);
                        }
                        else
                        {
                            exhaustManifolds.Add(allParts[i]);
                        }

                        break;
                    case PartType.intakeManifold:
                        if (allParts[i].EnginePartSupportsTurbo)
                        {
                            intakeManifolds_Turbo.Add(allParts[i]);
                        }
                        else
                        {
                            intakeManifolds.Add(allParts[i]);
                        }

                        break;

                    case PartType.cylinderHead:
                        cylinderHeads.Add(allParts[i]);
                        break;

                    case PartType.fuelSystem:
                        fuelSystems.Add(allParts[i]);
                        break;
                    case PartType.rotatingAssembly:
                        rotatingAssemblys.Add(allParts[i]);
                        break;
                    case PartType.camshaft:
                        camshafts.Add(allParts[i]);
                        break;
                    case PartType.inductionType:
                        inductionTypes.Add(allParts[i]);
                        break;
                }
            }

            isSetup = true;
        }

        public void ApplyEngineCustomisation(CarData data)
        {
            if (!isSetup)
            {
                Setup();
            }

            totalTorque = defaultMaxTorque;
            //need to check if part is turbo for manifolds
            isCurrentlyTurbo = isDefaultTurbo && data.inductionType == 0 || data.inductionType == 2;
            if (isCurrentlyTurbo)
            {
                SetPart(intakeManifolds_Turbo.ToArray(), data.intakeManifold);
                SetPart(exhaustManifolds_Turbo.ToArray(), data.exhaustHeader);
                SetPart(intakeManifolds.ToArray(), -1);
                SetPart(exhaustManifolds.ToArray(), -1);
            }
            else
            {
                SetPart(intakeManifolds.ToArray(), data.intakeManifold);
                SetPart(exhaustManifolds.ToArray(), data.exhaustHeader);
                SetPart(intakeManifolds_Turbo.ToArray(), -1);
                SetPart(exhaustManifolds_Turbo.ToArray(), -1);
            }

            SetPart(cylinderHeads.ToArray(), data.cylinderHead);
            SetPart(fuelSystems.ToArray(), data.fuelSystem);
            SetPart(rotatingAssemblys.ToArray(), data.rotatingAssembly);
            SetPart(camshafts.ToArray(), data.camshaft);

            Drivetrain playerDrivetrain = GetComponentInParent<Drivetrain>();
            playerDrivetrain.maxTorque = totalTorque;
            playerDrivetrain.minRPM = minRpm;
            playerDrivetrain.maxRPM = maxRpm;
            playerDrivetrain.torqueRPM = torqueRpm;
            playerDrivetrain.powerRPM = powerRpm;
            playerDrivetrain.maxPower = (totalTorque * maxRpm) / 5252;
            playerDrivetrain.enableTurbo = isCurrentlyTurbo;
            CarCustomisation customisation = playerDrivetrain.GetComponent<CarCustomisation>();
            customisation.SetVehicleClass(playerDrivetrain.maxPower);
            CarCustomisation.CalculateVehicleClass(playerDrivetrain.maxPower);
        }

        void SetPart(PartDetails[] details, int selected)
        {
            for (int i = 0; i < details.Length; i++)
            {
                //details[i].gameObject.SetActive(selected == i);
                if (details[i].linkedMesh != null)
                {
                    details[i].linkedMesh.SetActive(selected == i);
                }

                if (i == selected)
                {
                    if (details[i].enginePowerType == PartPowerType.Static)
                    {
                        totalTorque += details[i].EnginePartPower;
                    }
                    else
                    {
                        float percentageIncrease =
                            Mathf.RoundToInt(defaultMaxTorque * (details[i].EnginePartPower / 100));
                        totalTorque += percentageIncrease;
                    }
                }

                details[i].isActivePart = selected == i;
            }
        }
    }
}