using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SkillCheckManager : MonoBehaviour
    {

        [SerializeField] private int points;

        [Header("Near miss")]
        [Tooltip("The max distance around a traffic car to consider a near miss.")]
        [SerializeField] private float nearMissMaxDistance = 1;
        [SerializeField] private float nearMissNosBonus;

        [Header("Slip stream")]
        [Tooltip("The max distance behind a racer car that the player can be to consider it slip streaming.")]
        [SerializeField] private float slipStreamMaxDistance = 1;
        [Tooltip("A nos bonus to give each second while the player is within another racer cars slip stream bounds.")]
        [SerializeField] private float slipStreamNosBonus = 1;

        [Header("Air time")]
        [SerializeField] private float minimumSpeedForAirTimeKmh = 25;
        [Tooltip("A nos bonus to give each second while the player has 0 wheels touching the ground and moving at the minimum speed.")]
        [SerializeField] private float airTimeNosBonus = 1;

        [Header("Landing")]
        [SerializeField] private float minTimeInAirForLandingPoints = 0.3f;
        [SerializeField] private float landingNosBonus = 10;
        
        public int Points => points;

        private void Update()
        {
            CheckForNearMiss();
            CheckForSlipStream();
            CheckForAirTime();
        }

        private void CheckForNearMiss()
        {
            //TODO: do overlay box each frame and check for traffic cars
            // - if player was in radius last frame but is no longer in the radius, and there was not a collision - it is a near miss
        }

        private void CheckForSlipStream()
        {
            //TODO: do overlay box each frame and check for racers
        }

        private void CheckForAirTime()
        {
            //TODO: if all wheels are off the ground
            //set inAir = true
            //timeInAir++
            
            // - if goes from inAir to !inAir and timeInAir > minTimeInAirForLandingPoints - do landing
        }

        private void OnPerformNearMiss()
        {
            
        }
        
        private void OnPerformSlipStream()
        {
            
        }

        private void OnPerformAirTime()
        {
            
        }

        private void OnPerformLanding()
        {
            
        }

    }
}
