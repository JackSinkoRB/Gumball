using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class FuelManager
    {
        
        public const int MaxFuel = 10;

        public delegate void OnFuelChangeDelegate(int previousFuel, int newFuel);
        public static OnFuelChangeDelegate onFuelChange;
        
        public static int CurrentFuel
        {
            get => DataManager.Player.Get("Fuel.Current", MaxFuel); //initialise with max fuel
            private set => DataManager.Player.Set("Fuel.Current", value);
        }

        public static bool HasFuel(int amount = 1)
        {
            return CurrentFuel >= amount;
        }
        
        public static void ReplenishFuel()
        {
            SetFuel(MaxFuel);
        }
        
        public static void SetFuel(int amount)
        {
            if (CurrentFuel == amount)
                return; //already has this amount
            
            if (amount > MaxFuel)
            {
                Debug.LogError("Cannot set fuel above the maximum.");
                amount = MaxFuel;
            }

            if (amount < 0)
            {
                Debug.LogError("Cannot set fuel below 0.");
                amount = 0;
            }

            int previousFuel = CurrentFuel;
            CurrentFuel = amount;
            
            onFuelChange?.Invoke(previousFuel, amount);
        }
        
        public static void AddFuel(int amount = 1)
        {
            SetFuel(CurrentFuel + amount);
        }
        
        public static void TakeFuel(int amount = 1)
        {
            SetFuel(CurrentFuel - amount);
        }
        
    }
}
