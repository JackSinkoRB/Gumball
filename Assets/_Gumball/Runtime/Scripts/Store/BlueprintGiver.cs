using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    public class BlueprintGiver : MonoBehaviour
    {

        [HelpBox("Call the 'GiveBlueprints' method to give the blueprints to the player.", MessageType.Info, HelpBoxAttribute.Position.ABOVE)]
        [SerializeField] private BlueprintReward blueprintInfo;
        
        public void GiveBlueprints()
        {
            blueprintInfo.GiveReward();    
        }
        
    }
}
