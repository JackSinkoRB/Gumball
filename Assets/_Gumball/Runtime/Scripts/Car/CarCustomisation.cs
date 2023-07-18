using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
//use this to apply customisation
    public class CarCustomisation : MonoBehaviour
    {

        [Space]
        [Header("Default Values")] public string defaultEngine;
        public Material bodyColorMaterial;
        public Material carTrimMaterial;
        public Material carDetail;
        private Material bodyColorInstance;
        private Material carTrimInstance;
        public float defaultWheelScale = 1;

        [Space]
        [Header("Part Locations")] public Transform frontBars;
        public Transform rearBars;
        public Transform gaurds;
        public Transform bonnets;
        public Transform spoilers;
        public Transform sideSkirts;

        public Transform bonnetPivot;

        public Transform engineSpawn;

        [Space] [Header("Wheel Hub Refrences")]
        public Transform wheelFL;

        public Transform wheelFR;
        public Transform wheelRL;
        public Transform wheelRR;
        public Wheel[] wheelRoots;
        private string currentWheelType = "";
        [HideInInspector] public EngineCustomisation spawnedEngine;
        List<GameObject> spawnedWheels = new List<GameObject>();
        List<TireModification> spawnedTires = new List<TireModification>();
        public static string currentVehicleClass;
        public string vehicleClass;
        private CarData targetData; //set private after testing

        public CarData _targetData()
        {
            return targetData;
        }

        public void ApplyVehicleChanges(CarData newData, bool updateEngine = true, bool updateCosmetic = true)
        {
            targetData = newData;
            if (updateCosmetic)
            {
                ApplyCosmeticParts();
                ApplyWheelSetup();
                ApplyPaint();
            }

            if (updateEngine)
            {
                ApplyEngineChanges();
            }

        }

        void ApplyEngineChanges()
        {
            string targetEngine = null;
            if (targetData.engineType == null || targetData.engineType == "")
            {
                targetEngine = defaultEngine;
            }
            else
            {
                targetEngine = targetData.engineType;
            }

            if (spawnedEngine != null)
            {
                if (targetEngine == spawnedEngine.engineName) //engine is the same so we dont need to spawn a new one
                {
                    spawnedEngine.ApplyEngineCustomisation(targetData);
                    return;
                }

                Destroy(spawnedEngine.gameObject);
            }


            GameObject newEngine = (GameObject)Instantiate(Resources.Load("Engines/" + targetEngine), engineSpawn);
            spawnedEngine = newEngine.GetComponent<EngineCustomisation>();
            spawnedEngine.ApplyEngineCustomisation(targetData);
        }

        void ApplyCosmeticParts()
        {
            SetParts(frontBars, targetData.frontBars);
            SetParts(rearBars, targetData.rearBars);
            SetParts(gaurds, targetData.gaurds);
            SetParts(bonnets, targetData.bonnets);
            SetParts(spoilers, targetData.spoilers);
            SetParts(sideSkirts, targetData.sideSkirts);
        }

//wheel setup
        void ApplyWheelSetup()
        {
            if (targetData.wheelType != currentWheelType) //create new wheels
            {
                if (spawnedWheels.Count > 0)
                {
                    for (int i = 0; i < spawnedWheels.Count; i++)
                    {
                        Destroy(spawnedWheels[i]);
                    }

                }

                spawnedTires.Clear();
                spawnedWheels.Clear();

                GameObject targetWheelModel =
                    (GameObject)Resources.Load("Wheels/" +
                                               targetData.wheelType); //this can be changed to support front/rear
                CreateWheel(wheelFL, targetWheelModel, false, false);
                CreateWheel(wheelFR, targetWheelModel, true, false);
                CreateWheel(wheelRL, targetWheelModel, false, true);
                CreateWheel(wheelRR, targetWheelModel, true, true);
            }


            for (int i = 0; i < wheelRoots.Length; i++)
            {
                wheelRoots[i].offset = targetData.wheelOffset;
                Vector3 pos = wheelRoots[i].transform.localPosition;
                float scaleVal = (wheelRoots[i].isRear ? targetData.wheelSizeRear : targetData.wheelSize);
                Vector3 scale = new Vector3(defaultWheelScale + scaleVal, defaultWheelScale + scaleVal,
                    defaultWheelScale + scaleVal);
                pos.y = wheelRoots[i].initialSusOffset - targetData.suspensionHeight;
                float offsetVal = wheelRoots[i].isLeft ? 1 : -1;
                pos.x = wheelRoots[i].initialXOffset -
                        (offsetVal * (wheelRoots[i].isRear ? targetData.wheelOffsetRear : targetData.wheelOffset));
                //wheelRoots[i].offset = (wheelRoots[i].isRear ? targetData.wheelOffsetRear : targetData.wheelOffset);

                SetTire(spawnedTires[i], wheelRoots[i].isRear);
                //tire.SetSize(tWidth, tSize);
                float radius = 0.33f;
                radius += wheelRoots[i].isRear ? (targetData.tireSizeRear / 3.14f) : (targetData.tireSize / 3.14f);
                radius += wheelRoots[i].isRear ? (targetData.wheelSizeRear / 3.14f) : (targetData.wheelSize / 3.14f);
                radius -= (1 - defaultWheelScale) / 3.14f;
                wheelRoots[i].radius = radius;
                wheelRoots[i].transform.localPosition = pos;
                wheelRoots[i].camber = wheelRoots[i].isRear ? targetData.wheelCamberRear : targetData.wheelCamber;
                wheelRoots[i].transform.localScale = scale;
            }
        }

        void CreateWheel(Transform target, GameObject model, bool isRight, bool isRear)
        {
            GameObject newWheel = Instantiate(model, target);
            string targetTirePath = "Wheels/" + (isRight ? "_Tire_R" : "_Tire_L");
            GameObject newTire = (GameObject)Instantiate(Resources.Load(targetTirePath), newWheel.transform);
            spawnedTires.Add(newTire.GetComponent<TireModification>());
            //SetTire(tire, isRear);
            //Debug.Log("Width " + tWidth);
            //Debug.Log("Height " + tSize);

            if (isRight)
            {
                newWheel.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            }


            spawnedWheels.Add(newWheel);
        }

        void SetTire(TireModification tire, bool isRear)
        {
            float tSize = isRear ? targetData.tireSizeRear : targetData.tireSize;
            float tWidth = isRear ? targetData.tireWidthRear : targetData.tireWidth;
            tire.width = 1 + tWidth;
            tire.height = 1 + tSize;
            tire.ApplyChanges(tWidth, tSize);
        }
// end wheel setup

        private MeshRenderer[] paintPanels;

        void ApplyPaint()
        {
            if (bodyColorInstance == null)
            {
                //create new
                bodyColorInstance = Instantiate(bodyColorMaterial);
            }

            if (carTrimInstance == null)
            {
                //create new
                carTrimInstance = Instantiate(carTrimMaterial);
            }

            bodyColorInstance.SetColor(ColorTarget._Color.ToString(), targetData.mainBodyColor.ToColor());
            bodyColorInstance.SetColor(ColorTarget._InnerCol.ToString(), targetData.mainBodyInnerColor.ToColor());
            bodyColorInstance.SetColor(ColorTarget._OuterCol.ToString(), targetData.mainBodyOuterColor.ToColor());
            bodyColorInstance.SetFloat("_Gloss", targetData.mainBodyGloss);
            bodyColorInstance.SetFloat("_Metal", targetData.mainBodyMetallic);

            MeshRenderer[] paintPanels = transform.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < paintPanels.Length; i++)
            {
                MeshRenderer renderer = paintPanels[i];
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;

                    for (int j = 0; j < materials.Length; j++)
                    {
                        Material currentMaterial = materials[j];

                        // Check if the material has the same shader as the original material
                        if (currentMaterial.shader == bodyColorMaterial.shader)
                        {
                            // Replace the material with the replacement material
                            materials[j] = bodyColorInstance;
                        }

                        if (currentMaterial.shader == carDetail.shader)
                        {
                            materials[j] = carDetail;
                        }
                    }

                    // Apply the updated materials array to the renderer
                    renderer.materials = materials;
                }
            }

        }



        void SetParts(Transform parent, int targetActive)
        {
            PartDetails[] parts = parent.GetComponentsInChildren<PartDetails>(true);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].gameObject.SetActive(i == targetActive ? true : false);
            }
        }

        private static Dictionary<string, float> classThresholds = new Dictionary<string, float>()
        {
            { "S", 1000f },
            { "A+", 500f },
            { "A", 300f },
            { "B+", 200f },
            { "B", 160 },
            { "C", 0f }
        };

        public static string CalculateVehicleClass(float horsePower)
        {
            string newClass = "-";
            foreach (var threshold in classThresholds)
            {
                if (horsePower >= threshold.Value)
                {
                    newClass = threshold.Key;
                }
            }

            currentVehicleClass = newClass;
            return newClass;
        }

        public void SetVehicleClass(float horsePower)
        {
            string newClass = "-";
            foreach (var threshold in classThresholds)
            {
                if (horsePower >= threshold.Value)
                {
                    newClass = threshold.Key;
                }
            }

            vehicleClass = newClass;

        }

        public void SetBrakeLights(bool isOn)
        {
            carDetail.SetFloat("_Brake", isOn ? 1 : 0);
        }

    }
}