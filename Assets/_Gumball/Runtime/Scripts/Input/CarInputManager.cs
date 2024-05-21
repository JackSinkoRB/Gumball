using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarInputManager : ActionMapManager
    {

        public VirtualInputActionFloat Steering { get; private set; }
        public VirtualInputActionButton Accelerate { get; private set; }
        public VirtualInputActionButton Brake { get; private set; }
        public VirtualInputActionButton Handbrake { get; private set; }
        public VirtualInputActionButton ShiftUp { get; private set; }
        public VirtualInputActionButton ShiftDown { get; private set; }
        
        public float SteeringInput => Steering?.Value ?? 0;

        protected override void Initialise()
        {
            base.Initialise();
            
            Steering = new VirtualInputActionFloat(GetOrCacheAction("Steering"));
            Accelerate = new VirtualInputActionButton(GetOrCacheAction("Accelerate"));
            Brake = new VirtualInputActionButton(GetOrCacheAction("Decelerate"));
            Handbrake = new VirtualInputActionButton(GetOrCacheAction("Handbrake"));
            ShiftUp = new VirtualInputActionButton(GetOrCacheAction("ShiftUp"));
            ShiftDown = new VirtualInputActionButton(GetOrCacheAction("ShiftDown"));
        }

        protected override string GetActionMapName()
        {
            return "Driving";
        }

    }
}
