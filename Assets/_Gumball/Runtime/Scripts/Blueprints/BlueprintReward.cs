using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct BlueprintReward
    {
            
        [SerializeField] private CarDataReference car;
        [SerializeField, MinValue(1)] private int blueprints;

        public string CarGUID => car?.GUID;
        public int Blueprints => blueprints;
        
        public void GiveReward()
        {
            if (car == null)
                return;
            
            BlueprintManager.Instance.AddBlueprints(CarGUID, blueprints);
        }
        
        public override string ToString()
        {
            return CarGUID == null ? "null" : $"{CarGUID}-{blueprints}";
        }
            
    }
}
