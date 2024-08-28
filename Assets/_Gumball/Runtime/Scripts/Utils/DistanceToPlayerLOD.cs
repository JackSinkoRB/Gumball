using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DistanceToPlayerLOD : MonoBehaviour
    {

        private const float timeBetweenChecks = 0.5f;

        [Serializable]
        private struct LODLevel
        {
            [HelpBox("The distance should be more than the previous levels distance.", position: HelpBoxAttribute.Position.ABOVE)]
            [SerializeField] private float distanceToActivate;
            [SerializeField] private List<Transform> transformsToEnable;
            
            public float DistanceToActivate => distanceToActivate;
            
            public void Enable(bool enable)
            {
                foreach (Transform transform in transformsToEnable)
                    transform.gameObject.SetActive(enable);
            }

            public void SetDistance(float distance)
            {
                distanceToActivate = distance;
            }
        }
        
        [SerializeField] private List<LODLevel> lods = new();

        [Header("Debugging")]
        [SerializeField, ReadOnly] private LODLevel currentLevel;

        private float timeSinceLastCheck;
        
        private void LateUpdate()
        {
            DoCheck();
        }

        private void DoCheck()
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck < timeBetweenChecks)
                return;
            
            timeSinceLastCheck = 0;

            LODLevel? desiredLevel = GetDesiredLevel();

            if (desiredLevel == null)
                return;

            SelectLod(desiredLevel.Value);
        }

        private LODLevel? GetDesiredLevel()
        {
            if (WarehouseManager.Instance.CurrentCar == null)
                return null;

            if (lods.Count == 0)
                return null;
            
            float distanceToPlayerSqr = Vector3.SqrMagnitude(WarehouseManager.Instance.CurrentCar.transform.position - transform.position);

            LODLevel closestLevel = lods[0];
            for (int index = 1; index < lods.Count; index++)
            {
                LODLevel level = lods[index];
                
                float distanceSqr = level.DistanceToActivate * level.DistanceToActivate;
                if (distanceSqr > distanceToPlayerSqr) //use the first level that is greater than the player's distance
                    return level;
            }

            return closestLevel;
        }

        private void SelectLod(LODLevel desiredLevel)
        {
            if (desiredLevel.Equals(currentLevel))
                return; //is desired level
            
            currentLevel = desiredLevel;

            //enable only the selected level
            foreach (LODLevel lod in lods)
                lod.Enable(lod.Equals(desiredLevel));
            
            Debug.Log($"Set LOD for {gameObject.name} to {desiredLevel}");
        }

    }
}
