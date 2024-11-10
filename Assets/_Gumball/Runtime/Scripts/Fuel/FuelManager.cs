using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Fuel Manager")]
    public class FuelManager : SingletonScriptable<FuelManager>
    {
        
        public delegate void OnFuelChangeDelegate(int previousFuel, int newFuel);
        public OnFuelChangeDelegate onFuelChange;
        
        [SerializeField] private int maxFuel = 10;
        [Tooltip("How often does the player get given a fuel automatically?")]
        [SerializeField] private SerializedTimeSpan timeBetweenFuelRegenerate = new(0, 15, 0);

        public PersistentCooldown RegenerateCycle { get; private set; }
        
        public int MaxFuel => maxFuel;
        public SerializedTimeSpan TimeBetweenFuelRegenerate => timeBetweenFuelRegenerate;
        
        public int CurrentFuel
        {
            get => DataManager.Player.Get("Fuel.Current", maxFuel); //initialise with max fuel
            private set => DataManager.Player.Set("Fuel.Current", value);
        }

        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();
            
            RegenerateCycle = new PersistentCooldown("FuelRegenerateCycle", timeBetweenFuelRegenerate.ToSeconds(), false);
            RegenerateCycle.onCycleComplete += RegenerateFuel;
            RegenerateCycle.Play();
        }

        public bool HasFuel(int amount = 1)
        {
            return CurrentFuel >= amount;
        }
        
        public void ReplenishFuel()
        {
            SetFuel(maxFuel);
        }
        
        public void SetFuel(int amount)
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
            
            if (CurrentFuel == amount)
                return; //already has this amount
            
            int previousFuel = CurrentFuel;
            CurrentFuel = amount;
            
            onFuelChange?.Invoke(previousFuel, amount);

            bool wasMaxFuel = previousFuel == maxFuel && CurrentFuel < maxFuel;
            if (wasMaxFuel)
            {
                //restart the regenerate timer
                RegenerateCycle.Restart();
            }
        }
        
        public void AddFuel(int amount = 1)
        {
            SetFuel(CurrentFuel + amount);
        }
        
        public void TakeFuel(int amount = 1)
        {
            SetFuel(CurrentFuel - amount);
        }
        
        private void RegenerateFuel()
        {
            if (CurrentFuel == maxFuel)
                return;
            
            AddFuel();
        }

    }
}
