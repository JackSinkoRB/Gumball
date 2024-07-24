using System;
using System.Collections;
using System.Collections.Generic;
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

            public float Progress => Mathf.Clamp01(current / goal);
            
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

            Dictionary<string, Listener> listenersTemp = listeners;
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
            
            Dictionary<string, Listener> listenersTemp = listeners;
            listenersTemp.Remove(listenerId);
            listeners = listenersTemp;
        }

        public void Track(float amount)
        {
            Dictionary<string, Listener> listenersTemp = listeners;
            foreach (string listenerId in listeners.Keys)
            {
                listenersTemp[listenerId].Track(amount);
                listeners = listenersTemp;
            }
        }
        
        public void SetListenerValues(float amount)
        {
            Dictionary<string, Listener> listenersTemp = listeners;
            foreach (string listenerId in listeners.Keys)
            {
                if (listenersTemp[listenerId].Progress >= 1)
                    continue; //already completed - leave as is
                
                listenersTemp[listenerId].SetValue(amount);
                listeners = listenersTemp;
            }
        }
        
    }
}
