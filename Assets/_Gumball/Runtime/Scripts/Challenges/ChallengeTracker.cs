using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Gumball
{
    public abstract class ChallengeTracker : ScriptableObject
    {

        [Serializable]
        public class Listener
        {
            
            [SerializeField] private float goal;
            [SerializeField] private float current;

            public float Goal => goal;
            public float Current => current;
            public float Progress => Mathf.Clamp01(current / goal);
            public bool IsComplete => Progress >= 1;

            public Listener(float goal)
            {
                this.goal = goal;
                current = 0;
            }

            public void Track(float amount)
            {
                current += amount;
            }
            
            public void SetValue(float amount)
            {
                current = amount;
            }

        }

        [SerializeField] private string displayName = "Challenge";
        [SerializeField] private Sprite icon;

        public string DisplayName => displayName;
        public Sprite Icon => icon;

        public virtual string GetValueFormatted(float value) => value.ToString(CultureInfo.InvariantCulture);
        
        protected Dictionary<string, Listener> listeners
        {
            get => DataManager.GameSessions.Get($"Challenges.Listeners.{GetType()}", new Dictionary<string, Listener>());
            set => DataManager.GameSessions.Set($"Challenges.Listeners.{GetType()}", value);
        }
        
        public virtual void LateUpdate()
        {
            
        }

        public virtual void OnInstanceLoaded()
        {
            
        }
        
        public Listener GetListener(string listenerId)
        {
            if (!listeners.ContainsKey(listenerId))
                return null;
            
            return listeners[listenerId];
        }
        
        public void StartListening(string listenerId, int goal)
        {
            if (listeners.ContainsKey(listenerId))
            {
                Debug.LogWarning($"Cannot start listening for {listenerId} because it is already listening.");
                return;
            }

            Dictionary<string, Listener> listenersTemp = new Dictionary<string, Listener>(listeners);
            listenersTemp[listenerId] = new Listener(goal);
            listeners = listenersTemp;
        }

        public void StopListening(string listenerId)
        {
            if (!listeners.ContainsKey(listenerId))
            {
                Debug.LogWarning($"Cannot stop listening for {listenerId} because it is not listening.");
                return;
            }
            
            Dictionary<string, Listener> listenersTemp = new Dictionary<string, Listener>(listeners);
            listenersTemp.Remove(listenerId);
            listeners = listenersTemp;
        }

        public void Track(float amount)
        {
            if (amount == 0)
                return;
            
            Dictionary<string, Listener> listenersTemp = new Dictionary<string, Listener>(listeners);
            foreach (string listenerId in listenersTemp.Keys)
            {
                listenersTemp[listenerId].Track(amount);
            }
            listeners = listenersTemp;
        }
        
        public void SetListenerValues(float amount)
        {
            Dictionary<string, Listener> listenersTemp = new Dictionary<string, Listener>(listeners);
            foreach (string listenerId in listenersTemp.Keys)
            {
                if (listenersTemp[listenerId].IsComplete)
                    continue; //already completed - leave as is
                
                listenersTemp[listenerId].SetValue(amount);
            }
            listeners = listenersTemp;
        }
        
    }
}
