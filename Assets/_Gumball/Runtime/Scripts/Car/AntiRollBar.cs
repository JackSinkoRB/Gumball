using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class AntiRollBar : MonoBehaviour
    {
        [Tooltip("The two wheels connected by the anti-roll bar. These should be on the same axle.")]
        public Wheel wheel1;
        [Tooltip("The two wheels connected by the anti-roll bar. These should be on the same axle.")]
        public Wheel wheel2;
	
        [Tooltip("Coeefficient determining how much force is transfered by the bar.")]
        public float coefficient = 5000;
	
        private void FixedUpdate () 
        {
            float force = (wheel1.compression - wheel2.compression) * coefficient;
            wheel1.suspensionForceInput =+ force;
            wheel2.suspensionForceInput =- force;
        }
    }
}
