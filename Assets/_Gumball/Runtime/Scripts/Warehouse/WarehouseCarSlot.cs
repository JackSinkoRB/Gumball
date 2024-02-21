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
        [SerializeField, ReadOnly] private CarManager currentCar;

        public PositionAndRotation CameraPosition => cameraPosition;

        public IEnumerator PopulateWithCar(int index, int id)
        {
            yield return WarehouseManager.Instance.SpawnCar(index, id, transform.position, transform.rotation, OnSpawnCar);
        }

        public void PopulateWithCar(CarManager carManager)
        {
            currentCar = carManager;
            carManager.Teleport(transform.position, transform.rotation);
        }

        private void OnSpawnCar(CarManager car)
        {
            currentCar = car;
        }
        
    }
}
