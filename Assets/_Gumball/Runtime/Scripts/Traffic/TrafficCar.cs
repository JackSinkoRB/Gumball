using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
#if UNITY_EDITOR
using Gumball.Editor;
#endif
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCar : AICar
    {
        
        public override void Initialise()
        {
            base.Initialise();
            
            TrafficCarSpawner.TrackCar(this);
        }

        protected void OnDisable()
        {
            if (!isInitialised)
                return; //didn't get a chance to initialise
            
            TrafficCarSpawner.UntrackCar(this);
        }

    }
}
