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

        private string currentActionMapName;

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

        public void SetActionMapName(string actionMapName)
        {
            if (currentActionMapName != null && currentActionMapName.Equals(actionMapName))
                return; //already selected
            
            currentActionMapName = actionMapName;
            
            actionsCached.Clear(); //reset so the cache can be updated
        }
        
        protected override string GetActionMapName()
        {
            return currentActionMapName;
        }

    }
}
