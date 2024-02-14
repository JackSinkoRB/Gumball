using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class Cooldown
    {
        
        private readonly float duration;
        private readonly bool obeyTimeScale;
        
        [SerializeField, ReadOnly] private float timeOfLast;
        private float currentTime => obeyTimeScale ? Time.time : Time.unscaledTime;
        private float timeSinceLast => currentTime - timeOfLast;
        
        public bool IsReady => timeSinceLast >= duration;

        public Cooldown(float duration, bool obeyTimeScale = true)
        {
            this.duration = duration;
            this.obeyTimeScale = obeyTimeScale;
        }
        
        public void Reset()
        {
            timeOfLast = currentTime;
        }
        
    }
}
