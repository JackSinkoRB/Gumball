using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class ObstacleRaycastLayer
    {

        private static SortRaycastByAngle sorter = new();
        
        [SerializeField] private ObstacleRaycast[] raycasts;

        private bool isInitialised;
        private ObstacleRaycast[] unblockedRaycasts;

        private void Initialise()
        {
            isInitialised = true;
            
            unblockedRaycasts = new ObstacleRaycast[raycasts.Length];
        }
        
        /// <summary>
        /// Performs all the raycasts, and searches the unblocked ones for the least angle.
        /// </summary>
        /// <returns>The unblocked raycast with the least angle, or null if all are blocked.</returns>
        public ObstacleRaycast GetUnblockedRaycastWithLeastAngle(Transform fromTransform, Vector3 targetPosition, Vector3 targetDirection = default)
        {
            if (!isInitialised)
                Initialise();

            int numberOfUnblocked = 0;
            
            //do the raycasts
            foreach (ObstacleRaycast raycast in raycasts)
            {
                raycast.DoRaycast(fromTransform, targetPosition, targetDirection);

                if (!raycast.IsBlocked)
                {
                    unblockedRaycasts[numberOfUnblocked] = raycast;
                    numberOfUnblocked++;
                }
            }

            if (numberOfUnblocked == 0)
                return null;

            if (numberOfUnblocked == 1)
                return unblockedRaycasts[0];
            
            //sorter by angle (ascending order - smallest first)
            Array.Sort(unblockedRaycasts, sorter);
            
            //then return the first element
            return unblockedRaycasts[0];
        }
        
        public class SortRaycastByAngle : IComparer<ObstacleRaycast>
        {
            public int Compare(ObstacleRaycast x, ObstacleRaycast y)
            {
                return x?.Angle.CompareTo(y?.Angle) ?? 0;
            }
        }
        
    }
}
