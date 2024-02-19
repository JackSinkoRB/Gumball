using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace Gumball
{
    [Obsolete]
    public class CarCustomisation : MonoBehaviour
    {


        [Header("Default Values")]
        public string defaultEngine;

        [HideInInspector] public EngineCustomisation spawnedEngine;

        private CarData targetData;

        public CarData _targetData()
        {
            return targetData;
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
        

        

        // private void SetupWheel(Wheel wheel)
        // {
        //     wheel.offset = targetData.wheelOffset;
        //     Vector3 pos = wheel.transform.localPosition;
        //     float scaleVal = (wheel.isRear ? targetData.wheelSizeRear : targetData.wheelSize);
        //     Vector3 scale = new Vector3(defaultWheelScale + scaleVal, defaultWheelScale + scaleVal,
        //         defaultWheelScale + scaleVal);
        //     pos.y = wheel.initialSusOffset - targetData.suspensionHeight;
        //     float offsetVal = wheel.isLeft ? 1 : -1;
        //     pos.x = wheel.initialXOffset -
        //             (offsetVal * (wheel.isRear ? targetData.wheelOffsetRear : targetData.wheelOffset));
        //
        //     float radius = 0.33f;
        //     radius += wheel.isRear ? (targetData.tireSizeRear / 3.14f) : (targetData.tireSize / 3.14f);
        //     radius += wheel.isRear ? (targetData.wheelSizeRear / 3.14f) : (targetData.wheelSize / 3.14f);
        //     radius -= (1 - defaultWheelScale) / 3.14f;
        //     wheel.radius = radius;
        //     wheel.transform.localPosition = pos;
        //     wheel.camber = wheel.isRear ? targetData.wheelCamberRear : targetData.wheelCamber;
        //     wheel.transform.localScale = scale;
        // }

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

    }
}