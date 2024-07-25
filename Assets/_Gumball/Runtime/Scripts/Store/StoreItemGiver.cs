using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class StoreItemGiver : MonoBehaviour
    {

        public void GiveStandardCurrency(int amount)
        {
            Currency.Standard.AddFunds(amount);
        }
        
        public void GivePremiumCurrency(int amount)
        {
            Currency.Premium.AddFunds(amount);
        }

        public void GiveSubPart(SubPart subPart)
        {
            subPart.SetUnlocked(true);
        }
        
        public void GiveCorePart(CorePart corePart)
        {
            corePart.SetUnlocked(true);
        }

        public void ReplenishFuel()
        {
            FuelManager.Instance.ReplenishFuel();
        }
        
    }
}
