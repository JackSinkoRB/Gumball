using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarParticlesController : MonoBehaviour
    {

        [SerializeField] private Vector3 positionOffset;
        
        private void LateUpdate()
        {
            MoveToPlayer();
        }

        private void MoveToPlayer()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
                return;
            
            transform.position = WarehouseManager.Instance.CurrentCar.transform.TransformPoint(positionOffset);
            transform.rotation = Quaternion.LookRotation(-WarehouseManager.Instance.CurrentCar.transform.forward, WarehouseManager.Instance.CurrentCar.transform.up);
        }
        
    }
}
