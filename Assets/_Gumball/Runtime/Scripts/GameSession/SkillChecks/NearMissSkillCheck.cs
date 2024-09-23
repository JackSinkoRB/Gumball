using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class NearMissSkillCheck : SkillCheck
    {

        [Header("Near miss")]
        [SerializeField] private NearMissSkillCheckUI leftUI;
        [SerializeField] private NearMissSkillCheckUI rightUI;
        [Tooltip("The minimum speed the player must be going for a near miss.")]
        [SerializeField] private float minSpeedKmh = 50;
        [Tooltip("The max distance around a traffic car to consider a near miss.")]
        [SerializeField] private float maxDistance = 1.7f;
        [Tooltip("How long (in seconds) after a collision should it wait to have no collisions before checking for a near miss?")]
        [SerializeField] private float minTimeWithNoCollision = 0.5f;
        [Space(5)]
        [SerializeField, ReadOnly] private float timeSinceLastCollision;
        
        private readonly Dictionary<AICar, float> timeTrafficCarEnteredRadius = new();
        private readonly Collider[] tempHolder = new Collider[50];

        public override void CheckIfPerformed()
        {
            if (WarehouseManager.Instance.CurrentCar.InCollision)
            {
                timeSinceLastCollision = 0;
                return;
            }

            timeSinceLastCollision += Time.deltaTime;

            if (timeSinceLastCollision < minTimeWithNoCollision)
                return;
            
            if (WarehouseManager.Instance.CurrentCar.SpeedKmh < minSpeedKmh)
                return;
            
            Vector3 halfExtents = new Vector3(WarehouseManager.Instance.CurrentCar.CarWidth / 2f + maxDistance, 1, WarehouseManager.Instance.CurrentCar.FrontOfCarPosition.z + maxDistance);
            
            int hits = Physics.OverlapBoxNonAlloc(WarehouseManager.Instance.CurrentCar.transform.position, halfExtents, tempHolder, WarehouseManager.Instance.CurrentCar.transform.rotation, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.TrafficCar));

            HashSet<AICar> previousCarsInRadius = new HashSet<AICar>(timeTrafficCarEnteredRadius.Keys);
            HashSet<AICar> carsInRadius = new();
            
            //track the cars that entered
            for (int count = 0; count < hits; count++)
            {
                AICar trafficCar = tempHolder[count].transform.GetComponentInAllParents<AICar>();
                carsInRadius.Add(trafficCar);
                
                if (!timeTrafficCarEnteredRadius.ContainsKey(trafficCar))
                {
                    //entered near miss radius
                    OnTrafficCarEnterNearMissRadius(trafficCar);
                }
            }

            //track the cars that exited
            foreach (AICar trafficCar in previousCarsInRadius)
            {
                if (!carsInRadius.Contains(trafficCar))
                {
                    //no longer in near miss radius
                    OnTrafficCarExitNearMissRadius(trafficCar);
                }
            }
            
#if UNITY_EDITOR
            BoxCastUtils.DrawBox(WarehouseManager.Instance.CurrentCar.transform.position, halfExtents, WarehouseManager.Instance.CurrentCar.transform.rotation, hits > 0 ? Color.yellow : Color.grey);
#endif
        }
        
        private void OnTrafficCarEnterNearMissRadius(AICar car)
        {
            timeTrafficCarEnteredRadius[car] = Time.time;
        }

        private void OnTrafficCarExitNearMissRadius(AICar car)
        {
            float timeSinceEntering = Time.time - timeTrafficCarEnteredRadius[car];
            bool hasBeenInCollisionSinceEntering = timeSinceEntering > timeSinceLastCollision;

            bool isLeftSide = WarehouseManager.Instance.CurrentCar.IsPositionOnLeft(car.transform.position); //is the car closer to the left direction, or the right direction?
            if (!hasBeenInCollisionSinceEntering)
            {
                OnPerformed();
                ShowSkillCheckUI(isLeftSide);
            }

            timeTrafficCarEnteredRadius.Remove(car);
        }

        private void ShowSkillCheckUI(bool leftSide)
        {
            if (leftSide)
                leftUI.Show(Mathf.RoundToInt(pointBonus));
            else
                rightUI.Show(Mathf.RoundToInt(pointBonus));
        }

    }
}
