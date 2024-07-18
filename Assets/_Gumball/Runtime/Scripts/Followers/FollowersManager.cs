using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class FollowersManager
    {

        private const int startingFollowers = 0;
        
        public delegate void OnFollowersChangeDelegate(int previousFollowers, int newFollowers);
        public static OnFollowersChangeDelegate onFollowersChange;
        
        public static int CurrentFollowers
        {
            get => DataManager.Player.Get("Followers.Current", startingFollowers);
            private set => DataManager.Player.Set("Followers.Current", value);
        }
        
        public static void SetFollowers(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("Cannot set fuel below 0.");
                amount = 0;
            }
            
            if (CurrentFollowers == amount)
                return; //already has this amount
            
            int previousFollowers = CurrentFollowers;
            CurrentFollowers = amount;
            
            onFollowersChange?.Invoke(previousFollowers, amount);
        }
        
        public static void AddFollowers(int amount)
        {
            SetFollowers(CurrentFollowers + amount);
        }
        
    }
}
