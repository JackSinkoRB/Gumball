using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class FuelManager
    {
        
        public const int MaxFuel = 10;
        
        /// <summary>
        /// How often does the player get given a fuel automatically?
        /// </summary>
        public const int MinutesBetweenFuelRegenerate = 15;
        
        public delegate void OnFuelChangeDelegate(int previousFuel, int newFuel);
        public static OnFuelChangeDelegate onFuelChange;

        public static PersistentCooldown RegenerateCycle { get; private set; }
        
        public static int CurrentFuel
        {
            get => DataManager.Player.Get("Fuel.Current", MaxFuel); //initialise with max fuel
            private set => DataManager.Player.Set("Fuel.Current", value);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeInitialise()
        {
            RegenerateCycle = new PersistentCooldown("FuelRegenerateCycle", MinutesBetweenFuelRegenerate * TimeUtils.SecondsInAMinute);
            RegenerateCycle.onCycleComplete += RegenerateFuel;
            RegenerateCycle.Play();
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
            
            if (CurrentFuel == amount)
                return; //already has this amount
            
            int previousFuel = CurrentFuel;
            CurrentFuel = amount;
            
            onFuelChange?.Invoke(previousFuel, amount);

            bool wasMaxFuel = previousFuel == MaxFuel && CurrentFuel < MaxFuel;
            if (wasMaxFuel)
            {
                //restart the regenerate timer
                RegenerateCycle.Restart();
            }
        }
        
        public static void AddFuel(int amount = 1)
        {
            SetFuel(CurrentFuel + amount);
        }
        
        public static void TakeFuel(int amount = 1)
        {
            SetFuel(CurrentFuel - amount);
        }
        
        private static void RegenerateFuel()
        {
            if (CurrentFuel == MaxFuel)
                return;
            
            AddFuel();
        }

    }
}
