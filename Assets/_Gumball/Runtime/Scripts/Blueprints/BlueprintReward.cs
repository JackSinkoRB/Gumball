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
            
        [SerializeField] private string carGUID;
        [SerializeField, MinValue(1)] private int blueprints;

        public string CarGUID => carGUID;
        public int Blueprints => blueprints;
        
        public void GiveReward()
        {
            BlueprintManager.Instance.AddBlueprints(carGUID, blueprints);
        }
        
        public override string ToString()
        {
            return $"{carGUID}-{blueprints}";
        }
            
    }
}
