using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarParticlesController : MonoBehaviour
    {

        [SerializeField] private Vector3 positionOffset;
        
        //TODO: experiment with changing particle color transparency and amount of particles depending on the player's speed
        
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
