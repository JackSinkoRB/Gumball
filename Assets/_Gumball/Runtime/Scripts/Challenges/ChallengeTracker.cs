using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public abstract class ChallengeTracker : ScriptableObject
    {

        [Serializable]
        public class Tracker
        {
            
            [SerializeField] private float goal;
            [SerializeField] private float current;

            public float Progress => Mathf.Clamp01(current / goal);
            
            public Tracker(float goal)
            {
                this.goal = goal;
                current = 0;
            }

            public void Track(float amount)
            {
                current += amount;
            }
            
            public void SetTracker(float amount)
            {
                current = amount;
            }

        }

        protected Dictionary<string, Tracker> trackers
        {
            get => DataManager.GameSessions.Get($"Challenges.Trackers.{GetType()}", new Dictionary<string, Tracker>());
            set => DataManager.GameSessions.Set($"Challenges.Trackers.{GetType()}", value);
        }

        public virtual void LateUpdate()
        {
            
        }

        public virtual void OnInstanceLoaded()
        {
            
        }
        
        public Tracker GetTracker(string trackerId)
        {
            if (!trackers.ContainsKey(trackerId))
                return null;
            
            return trackers[trackerId];
        }
        
        public void StartTracking(string trackerId, int goal)
        {
            if (trackers.ContainsKey(trackerId))
            {
                Debug.LogWarning($"Cannot start tracking for {trackerId} because it is already tracking.");
                return;
            }

            Dictionary<string, Tracker> trackersTemp = trackers;
            trackersTemp[trackerId] = new Tracker(goal);
            trackers = trackersTemp;
        }

        public void StopTracking(string trackerId)
        {
            if (!trackers.ContainsKey(trackerId))
            {
                Debug.LogWarning($"Cannot stop tracking for {trackerId} because it is not tracking.");
                return;
            }
            
            Dictionary<string, Tracker> trackersTemp = trackers;
            trackersTemp.Remove(trackerId);
            trackers = trackersTemp;
        }

        public void Track(float amount)
        {
            Dictionary<string, Tracker> trackersTemp = trackers;
            foreach (string trackerId in trackers.Keys)
            {
                trackersTemp[trackerId].Track(amount);
                trackers = trackersTemp;
            }
        }
        
        public void SetTracker(float amount)
        {
            Dictionary<string, Tracker> trackersTemp = trackers;
            foreach (string trackerId in trackers.Keys)
            {
                if (trackersTemp[trackerId].Progress >= 1)
                    continue; //already completed - leave as is
                
                trackersTemp[trackerId].SetTracker(amount);
                trackers = trackersTemp;
            }
        }
        
    }
}
