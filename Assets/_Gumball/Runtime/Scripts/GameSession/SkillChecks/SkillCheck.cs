using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public abstract class SkillCheck
    {

        public event Action onPerformed;
        
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [Space(5)]
        [SerializeField] protected float pointBonus;
        [SerializeField, Range(0,1)] protected float nosBonus;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private float pointsSinceSessionStart;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public float PointsSinceSessionStart => pointsSinceSessionStart;

        public abstract void CheckIfPerformed();

        public void ResetSessionPoints()
        {
            pointsSinceSessionStart = 0;
        }
        
        protected virtual void OnPerformed()
        {
            float pointsToAdd = GetPointsToAddWhenPerformed();
            SkillCheckManager.Instance.AddPoints(pointsToAdd);
            pointsSinceSessionStart += pointsToAdd;

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
