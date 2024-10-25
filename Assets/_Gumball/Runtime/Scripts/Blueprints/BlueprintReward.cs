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
            
        [SerializeField] private int carIndex;
        [SerializeField, MinValue(1)] private int blueprints;

        public int CarIndex => carIndex;
        public int Blueprints => blueprints;
        
        public void GiveReward()
        {
            BlueprintManager.Instance.AddBlueprints(carIndex, blueprints);
        }
            
    }
}
