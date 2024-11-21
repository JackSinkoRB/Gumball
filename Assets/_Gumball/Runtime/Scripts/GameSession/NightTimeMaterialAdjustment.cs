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

            GameSession.onSessionLoad -= OnSessionLoad;
            GameSession.onSessionLoad += OnSessionLoad;
        }

        private void OnSessionLoad(GameSession gameSession)
        {
#if UNITY_EDITOR
            //find all MeshRenderers in the scene
            MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
            
            foreach (MeshRenderer renderer in allRenderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Material material = renderer.sharedMaterials[i];
                    if (Array.Exists(materials, m => m == material))
                    {
                        // Create a fresh MaterialPropertyBlock for this renderer
                        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(propertyBlock, i);

                        // Set the properties
                        foreach (Modifier modifier in modifiers)
                            propertyBlock.SetFloat(modifier.Property, gameSession.IsNightTime ? modifier.NightValue : modifier.DayValue);

                        // Apply the MaterialPropertyBlock to the renderer
                        renderer.SetPropertyBlock(propertyBlock, i);
                    }
                }
            }
#else
            //just modify the material in a build to avoid searching for meshrenderers
            foreach (Material material in materials)
            {
                foreach (Modifier modifier in modifiers)
                    material.SetFloat(modifier.Property, gameSession.IsNightTime ? modifier.NightValue : modifier.DayValue);
            }
#endif
            
            Debug.Log($"[NightTimeMaterialAdjustment] Updated material values for session (is night time? {gameSession.IsNightTime})");
        }

    }
}
