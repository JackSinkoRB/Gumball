using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace Gumball
{
    public class CarCustomisation : MonoBehaviour
    {

        private static readonly int BrakeShaderID = Shader.PropertyToID("_Brake");

        [Header("Default Values")]
        public string defaultEngine;
        [SerializeField] private Material brakeLightsMaterial;

        [Header("Wheels")]
        public float defaultWheelScale = 1;
        public Transform wheelFL;
        public Transform wheelFR;
        public Transform wheelRL;
        public Transform wheelRR;
        public List<Wheel> wheelRoots = new();

        [SerializeField] private AssetReferenceGameObject wheelFrontAssetReference;
        [SerializeField] private AssetReferenceGameObject wheelRearAssetReference;
        [SerializeField] private AssetReferenceGameObject tyreLeftAssetReference;
        [SerializeField] private AssetReferenceGameObject tyreRightAssetReference;

        private readonly List<GameObject> spawnedWheels = new();
        private readonly List<TireModification> spawnedTyres = new();
        private AsyncOperationHandle<GameObject> tyreLeftHandle;
        private AsyncOperationHandle<GameObject> tyreRightHandle;
        
        [HideInInspector] public EngineCustomisation spawnedEngine;

        private CarData targetData;

        public CarData _targetData()
        {
            return targetData;
        }

        public IEnumerator ApplyVehicleChanges(CarData newData, bool updateEngine = true, bool updateCosmetic = true)
        {
            targetData = newData;
            if (updateCosmetic)
            {
                ApplyCosmeticParts();
                yield return ApplyWheelSetup();
                //ApplyPaint();
            }

            if (updateEngine)
            {
                ApplyEngineChanges(); //TODO
            }
        }

        private void ApplyEngineChanges()
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


            //TODO: Engine customisation
            // GameObject newEngine = (GameObject)Instantiate(Resources.Load("Engines/" + targetEngine), engineSpawn);
            // spawnedEngine = newEngine.GetComponent<EngineCustomisation>();
            // spawnedEngine.ApplyEngineCustomisation(targetData);
        }

        private void ApplyCosmeticParts()
        {
            // SetParts(frontBars, targetData.frontBars);
            // SetParts(rearBars, targetData.rearBars);
            // SetParts(guards, targetData.gaurds);
            // SetParts(bonnets, targetData.bonnets);
            // SetParts(spoilers, targetData.spoilers);
            // SetParts(sideSkirts, targetData.sideSkirts);
        }

        private IEnumerator ApplyWheelSetup()
        {
            RemoveWheels();
            
            //spawn the wheel model using addressables
            AsyncOperationHandle<GameObject> handleFront = Addressables.LoadAssetAsync<GameObject>(wheelFrontAssetReference);
            AsyncOperationHandle<GameObject> handleRear = wheelFrontAssetReference == wheelRearAssetReference ? handleFront : Addressables.LoadAssetAsync<GameObject>(wheelRearAssetReference);
            
            if (!tyreLeftHandle.IsValid())
                tyreLeftHandle = Addressables.LoadAssetAsync<GameObject>(tyreLeftAssetReference);

            if (!tyreRightHandle.IsValid())
                tyreRightHandle = Addressables.LoadAssetAsync<GameObject>(tyreRightAssetReference);

            //wait for all 3 handles to complete
            AsyncOperationHandle[] handles = { handleFront, handleRear, tyreLeftHandle, tyreRightHandle };
            yield return new WaitUntil(() => handles.AreAllComplete());

            CreateWheel(wheelFL, handleFront, false, false);
            CreateWheel(wheelFR, handleFront, true, false);
            CreateWheel(wheelRL, handleRear, false, true);
            CreateWheel(wheelRR, handleRear, true, true);
        }

        private void RemoveWheels()
        {
            foreach (GameObject wheel in spawnedWheels)
            {
                Destroy(wheel);
            }

            spawnedTyres.Clear();
            spawnedWheels.Clear();
        }

        private void CreateWheel(Transform target, AsyncOperationHandle<GameObject> handle, bool isRight, bool isRear)
        {
            GameObject model = handle.Result;
            GameObject wheel = Instantiate(model, target, false);
            wheel.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            
            int wheelIndex = spawnedWheels.Count;
            spawnedWheels.Add(wheel);
            
            CreateTyre(wheel.transform, isRight, isRear);
            
            SetupWheel(wheelRoots[wheelIndex]);
            
            if (isRight)
                wheel.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }

        private void SetupWheel(Wheel wheel)
        {
            wheel.offset = targetData.wheelOffset;
            Vector3 pos = wheel.transform.localPosition;
            float scaleVal = (wheel.isRear ? targetData.wheelSizeRear : targetData.wheelSize);
            Vector3 scale = new Vector3(defaultWheelScale + scaleVal, defaultWheelScale + scaleVal,
                defaultWheelScale + scaleVal);
            pos.y = wheel.initialSusOffset - targetData.suspensionHeight;
            float offsetVal = wheel.isLeft ? 1 : -1;
            pos.x = wheel.initialXOffset -
                    (offsetVal * (wheel.isRear ? targetData.wheelOffsetRear : targetData.wheelOffset));

            float radius = 0.33f;
            radius += wheel.isRear ? (targetData.tireSizeRear / 3.14f) : (targetData.tireSize / 3.14f);
            radius += wheel.isRear ? (targetData.wheelSizeRear / 3.14f) : (targetData.wheelSize / 3.14f);
            radius -= (1 - defaultWheelScale) / 3.14f;
            wheel.radius = radius;
            wheel.transform.localPosition = pos;
            wheel.camber = wheel.isRear ? targetData.wheelCamberRear : targetData.wheelCamber;
            wheel.transform.localScale = scale;
        }

        private void CreateTyre(Transform wheel, bool isRight, bool isRear)
        {
            AsyncOperationHandle<GameObject> handle = isRight ? tyreRightHandle : tyreLeftHandle;
            
            GameObject tyre = Instantiate(handle.Result, wheel, false);
            tyre.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);

            TireModification tyreModification = tyre.GetComponent<TireModification>();
            spawnedTyres.Add(tyreModification);
                
            SetTire(tyreModification, isRear);
        }

        private void SetTire(TireModification tire, bool isRear)
        {
            float tSize = isRear ? targetData.tireSizeRear : targetData.tireSize;
            float tWidth = isRear ? targetData.tireWidthRear : targetData.tireWidth;
            tire.width = 1 + tWidth;
            tire.height = 1 + tSize;
            tire.ApplyChanges(tWidth, tSize);
        }

        private MeshRenderer[] paintPanels;

        private void ApplyPaint()
        {
            // if (bodyColorInstance == null)
            // {
            //     //create new
            //     bodyColorInstance = Instantiate(bodyColorMaterial);
            // }
            //
            // if (carTrimInstance == null)
            // {
            //     //create new
            //     carTrimInstance = Instantiate(carTrimMaterial);
            // }
            //
            // bodyColorInstance.SetColor(ColorTarget._Color.ToString(), targetData.mainBodyColor.ToColor());
            // bodyColorInstance.SetColor(ColorTarget._InnerCol.ToString(), targetData.mainBodyInnerColor.ToColor());
            // bodyColorInstance.SetColor(ColorTarget._OuterCol.ToString(), targetData.mainBodyOuterColor.ToColor());
            // bodyColorInstance.SetFloat("_Gloss", targetData.mainBodyGloss);
            // bodyColorInstance.SetFloat("_Metal", targetData.mainBodyMetallic);
            //
            // MeshRenderer[] paintPanels = transform.GetComponentsInChildren<MeshRenderer>(true);
            // for (int i = 0; i < paintPanels.Length; i++)
            // {
            //     MeshRenderer renderer = paintPanels[i];
            //     if (renderer != null)
            //     {
            //         Material[] materials = renderer.materials;
            //
            //         for (int j = 0; j < materials.Length; j++)
            //         {
            //             Material currentMaterial = materials[j];
            //
            //             // Check if the material has the same shader as the original material
            //             if (currentMaterial.shader == bodyColorMaterial.shader)
            //             {
            //                 // Replace the material with the replacement material
            //                 materials[j] = bodyColorInstance;
            //             }
            //
            //             if (currentMaterial.shader == carDetail.shader)
            //             {
            //                 materials[j] = carDetail;
            //             }
            //         }
            //
            //         // Apply the updated materials array to the renderer
            //         renderer.materials = materials;
            //     }
            // }
        }
        
        private void SetParts(Transform parent, int targetActive)
        {
            PartDetails[] parts = parent.GetComponentsInChildren<PartDetails>(true);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].gameObject.SetActive(i == targetActive ? true : false);
            }
        }

        public void SetBrakeLights(bool isOn)
        {
            brakeLightsMaterial.SetFloat(BrakeShaderID, isOn ? 1 : 0);
        }

    }
}