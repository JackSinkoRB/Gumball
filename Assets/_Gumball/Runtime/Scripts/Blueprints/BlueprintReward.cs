using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct BlueprintReward
    {
            
        [SerializeField] private int carIndex;
        [SerializeField] private int blueprints;

        public void GiveReward()
        {
            BlueprintManager.Instance.AddBlueprints(carIndex, blueprints);
        }
            
    }
}
