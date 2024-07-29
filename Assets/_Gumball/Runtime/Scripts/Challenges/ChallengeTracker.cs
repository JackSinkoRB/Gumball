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

        protected Dictionary<string, Listener> listeners => DataManager.GameSessions.Get($"Challenges.Listeners.{GetType()}", new Dictionary<string, Listener>());

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

            listeners[listenerId] = new Listener(goal);
            DataManager.GameSessions.SetDirty();
        }

        public void StopListening(string listenerId)
        {
            if (!listeners.ContainsKey(listenerId))
            {
                Debug.LogWarning($"Cannot stop listening for {listenerId} because it is not listening.");
                return;
            }
            
            listeners.Remove(listenerId);
            DataManager.GameSessions.SetDirty();
        }

        public void Track(float amount)
        {
            if (amount == 0)
                return;
            
            foreach (string listenerId in listeners.Keys)
            {
                listeners[listenerId].Track(amount);
            }
            DataManager.GameSessions.SetDirty();
        }
        
        public void SetListenerValues(float amount)
        {
            foreach (string listenerId in listeners.Keys)
            {
                if (listeners[listenerId].Progress >= 1)
                    continue; //already completed - leave as is
                
                listeners[listenerId].SetValue(amount);
            }
            DataManager.GameSessions.SetDirty();
        }
        
    }
}
