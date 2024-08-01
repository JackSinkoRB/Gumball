using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Knockout")]
    public class KnockoutGameSession : GameSession
    {

        [HelpBox("There is 1 knockout position per racer, with the Race Distance being the last knockout position.", MessageType.Info, HelpBoxAttribute.Position.ABOVE)]
        [SerializeField] private float[] knockoutPositions;

        public override string GetName()
        {
            return "Knockout";
        }
        
        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<KnockoutSessionPanel>();
        }

        protected override GameSessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<KnockoutSessionEndPanel>();
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            SetKnockoutDistancesRelativeToRacers();
            ForceSetLastKnockoutPosition();
        }

        private void ForceSetLastKnockoutPosition()
        {
            if (knockoutPositions.Length == 0)
                return;
            
            knockoutPositions[^1] = raceDistanceMetres;
        }

        private void SetKnockoutDistancesRelativeToRacers()
        {
            int desiredKnockoutPositions = racerData.Length - 1;

            if (knockoutPositions.Length == desiredKnockoutPositions)
                return;
            
            if (desiredKnockoutPositions <= 0)
            {
                knockoutPositions = Array.Empty<float>();
                return;
            }

            knockoutPositions = new float[desiredKnockoutPositions];
            for (int index = 0; index < knockoutPositions.Length; index++)
            {
                float knockoutPosition = knockoutPositions[index];
                if (knockoutPosition == 0)
                    knockoutPosition = Mathf.RoundToInt((index + 1) * (raceDistanceMetres / knockoutPositions.Length));
                
                knockoutPositions[index] = knockoutPosition;
            }
        }
#endif
        
    }
}
