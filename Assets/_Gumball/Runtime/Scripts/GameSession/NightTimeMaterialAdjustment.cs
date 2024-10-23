using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Night Time Material Adjustment")]
    public class NightTimeMaterialAdjustment : SingletonScriptable<NightTimeMaterialAdjustment>
    {

        [Serializable]
        private struct Modifier
        {
            [SerializeField] private string property;
            [SerializeField] private float dayValue;
            [SerializeField] private float nightValue;

            public string Property => property;
            public float DayValue => dayValue;
            public float NightValue => nightValue;
        }
        
        [SerializeField] private Material[] materials;
        [SerializeField] private Modifier[] modifiers;

        protected override void OnInstanceLoaded()
        {
            base.OnInstanceLoaded();

            GameSession.onSessionStart += OnSessionStart;
        }

        private void OnSessionStart(GameSession gameSession)
        {
            foreach (Material material in materials)
            {
                foreach (Modifier modifier in modifiers)
                    material.SetFloat(modifier.Property, gameSession.IsNightTime ? modifier.NightValue : modifier.DayValue);
            }
        }

    }
}
