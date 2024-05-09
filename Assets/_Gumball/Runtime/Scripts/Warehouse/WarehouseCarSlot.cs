using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class WarehouseCarSlot : MonoBehaviour
    {
        
        [SerializeField] private PositionAndRotation cameraPosition;
        [SerializeField, ReadOnly] private AICar currentCar;

        public PositionAndRotation CameraPosition => cameraPosition;

        public IEnumerator PopulateWithCar(int index)
        {
            yield return WarehouseManager.Instance.SpawnCar(index, transform.position, transform.rotation, OnSpawnCar);
        }

        public void PopulateWithCar(AICar car)
        {
            currentCar = car;
            car.Teleport(transform.position, transform.rotation);
        }

        public void OnSelected()
        {
            if (currentCar != null)
                WarehouseManager.Instance.SetCurrentCar(currentCar);
        }

        private void OnSpawnCar(AICar car)
        {
            currentCar = car;
        }
        
    }
}
