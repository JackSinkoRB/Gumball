using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class CarWheelsManager : MonoBehaviour
    {

        [SerializeField] private Wheel[] wheels;
        [SerializeField] private Wheel frontLeftWheel;
        [SerializeField] private Wheel frontRightWheel;
        [SerializeField] private Wheel rearLeftWheel;
        [SerializeField] private Wheel rearRightWheel;
        
        [SerializeField] private AssetReferenceGameObject wheelFrontAssetReference;
        [SerializeField] private AssetReferenceGameObject wheelRearAssetReference;
        [SerializeField] private AssetReferenceGameObject tyreLeftAssetReference;
        [SerializeField] private AssetReferenceGameObject tyreRightAssetReference;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<GameObject> spawnedWheels = new();
        [SerializeField, ReadOnly] private List<TireModification> spawnedTyres = new();
        
        private AsyncOperationHandle<GameObject> tyreLeftHandle;
        private AsyncOperationHandle<GameObject> tyreRightHandle;

        public Wheel[] Wheels => wheels;
        public Wheel FrontLeftWheel => frontLeftWheel;
        public Wheel FrontRightWheel => frontRightWheel;
        public Wheel RearLeftWheel => rearLeftWheel;
        public Wheel RearRightWheel => rearRightWheel;
        
        public IEnumerator Initialise()
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

            CreateWheel(frontLeftWheel.transform, handleFront, false, false);
            CreateWheel(frontRightWheel.transform, handleFront, true, false);
            CreateWheel(rearLeftWheel.transform, handleRear, false, true);
            CreateWheel(rearRightWheel.transform, handleRear, true, true);
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

        private void CreateWheel(Transform parent, AsyncOperationHandle<GameObject> handle, bool isRight, bool isRear)
        {
            GameObject model = handle.Result;
            GameObject wheel = Instantiate(model, parent, false);
            wheel.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            
            spawnedWheels.Add(wheel);
            
            CreateTyre(wheel.transform, isRight, isRear);

            //int wheelIndex = spawnedWheels.Count;
            //SetupWheel(wheelRoots[wheelIndex]);
            
            if (isRight)
                wheel.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
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
            float tSize = 0; //isRear ? targetData.tireSizeRear : targetData.tireSize;
            float tWidth = 0; //isRear ? targetData.tireWidthRear : targetData.tireWidth;
            tire.width = 1 + tWidth;
            tire.height = 1 + tSize;
            tire.ApplyChanges(tWidth, tSize);
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
        
    }
}
