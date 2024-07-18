using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public abstract class SkillCheck
    {

        public event Action onPerformed;

        [SerializeField] protected TextMeshProUGUI label;

        [SerializeField] protected float pointBonus;
        [SerializeField, Range(0,1)] protected float nosBonus;

        public TextMeshProUGUI Label => label;

        public abstract void CheckIfPerformed();

        protected virtual void OnPerformed()
        {
            SkillCheckManager.Instance.AddPoints(GetPointsToAddWhenPerformed());
            WarehouseManager.Instance.CurrentCar.NosManager.AddNos(GetNosToAddWhenPerformed());
            
            onPerformed?.Invoke();
        }

        protected virtual float GetPointsToAddWhenPerformed()
        {
            return pointBonus;
        }

        protected virtual float GetNosToAddWhenPerformed()
        {
            return nosBonus;
        }
        
    }
}
