using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class FuelManager
    {
        
        private const int maxFuel = 10;

        public delegate void OnFuelChangeDelegate(int previousFuel, int newFuel);
        public static OnFuelChangeDelegate onFuelChange;
        
        public static int CurrentFuel
        {
            get => DataManager.Player.Get("Fuel.Current", maxFuel); //initialise with max fuel
            private set => DataManager.Player.Set("Fuel.Current", value);
        }
        
        public static void SetFuel(int amount)
        {
            if (amount > maxFuel)
            {
                Debug.LogError("Cannot set fuel above the maximum.");
                amount = maxFuel;
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
        
    }
}
