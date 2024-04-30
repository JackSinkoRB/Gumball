using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class PaintModification : MonoBehaviour
    {
        
        public enum PaintMode
        {
            SIMPLE,
            ADVANCED
        }
                
        //caching:
        public static readonly int BaseColorShaderID = Shader.PropertyToID("_BaseColor");
        public static readonly int EmissionShaderID = Shader.PropertyToID("_Emission");
        public static readonly int ClearCoatSmoothnessShaderID = Shader.PropertyToID("_ClearCoatSmoothness");
        public static readonly int ClearCoatShaderID = Shader.PropertyToID("_ClearCoat");
        public static readonly int SmoothnessShaderID = Shader.PropertyToID("_Smoothness");
        public static readonly int MetallicShaderID = Shader.PropertyToID("_Metallic");
        
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private ColourSwatch[] swatchPresets;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;
        [SerializeField, ReadOnly] private MeshRenderer[] colourableBodyParts;

        private string saveKey => $"{carBelongsTo.SaveKey}.Paint";

        public MeshRenderer[] ColourableBodyParts => colourableBodyParts;
        public ColourSwatch[] SwatchPresets => swatchPresets;
        public PaintMode CurrentBodyPaintMode => GetCurrentSwatchIndexInPresets() == -1 ? PaintMode.ADVANCED : PaintMode.SIMPLE;

        public ColourSwatchSerialized CurrentBodyColour
        {
            get => DataManager.Cars.Get<ColourSwatchSerialized>($"{saveKey}.BodyColour");
            set => DataManager.Cars.Set($"{saveKey}.BodyColour", value);
        }
        
        public int CurrentSelectedPresetIndex
        {
            get => DataManager.Cars.Get($"{saveKey}.SelectedPreset", 0);
            set => DataManager.Cars.Set($"{saveKey}.SelectedPreset", value);
        }

#if UNITY_EDITOR
        [Header("Testing")]
        [SerializeField] private ColourSwatch testSwatch;
        
        [ButtonMethod]
        public void ApplyTestSwatch()
        {
            if (carBelongsTo == null)
                carBelongsTo = transform.GetComponentInAllParents<AICar>();
            
            FindBodyParts();
            ApplySwatch(testSwatch);
        }
        
        [ButtonMethod]
        public void ClearTestSwatch()
        {
            FindBodyParts();
            foreach (MeshRenderer meshRenderer in colourableBodyParts)
            {
                meshRenderer.sharedMaterial = bodyMaterial;
            }
        }
#endif

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
            
            FindBodyParts();

            LoadFromSave();
        }

        public void ApplySwatch(ColourSwatch swatch)
        {
            ApplySwatch(swatch.Serialize());
        }
        
        public void ApplySwatch(ColourSwatchSerialized swatch)
        {
            EnsureMaterialIsCopy();
            
            foreach (MeshRenderer meshRenderer in colourableBodyParts)
            {
                meshRenderer.sharedMaterial.SetColor(BaseColorShaderID, swatch.Color.ToColor());
                meshRenderer.sharedMaterial.SetColor(EmissionShaderID, swatch.Emission.ToColor());
                
                meshRenderer.sharedMaterial.SetFloat(MetallicShaderID, swatch.Metallic);
                meshRenderer.sharedMaterial.SetFloat(SmoothnessShaderID, swatch.Smoothness);
                meshRenderer.sharedMaterial.SetFloat(ClearCoatShaderID, swatch.ClearCoat);
                meshRenderer.sharedMaterial.SetFloat(ClearCoatSmoothnessShaderID, swatch.ClearCoatSmoothness);
            }

            CurrentBodyColour = swatch;
        }

        public void LoadFromSave()
        {
            if (!DataManager.Cars.HasKey($"{saveKey}.BodyColour"))
            {
                ApplySwatch(swatchPresets[0]); //apply the default
                return;
            }

            ColourSwatchSerialized saveData = DataManager.Cars.Get<ColourSwatchSerialized>($"{saveKey}.BodyColour");
            ApplySwatch(saveData);
        }

        /// <returns>Gets the index of the current swatch if it exists in the presets, else returns -1.</returns>
        public int GetCurrentSwatchIndexInPresets()
        {
            for (int index = 0; index < swatchPresets.Length; index++)
            {
                ColourSwatch colourSwatch = swatchPresets[index];
                if (CurrentBodyColour.Equals(colourSwatch))
                    return index;
            }
            
            return -1;
        }

        /// <summary>
        /// Finds all the body parts and assigns a single material instance that can be modified.
        /// </summary>
        private void FindBodyParts()
        {
            List<MeshRenderer> meshRenderers = carBelongsTo.transform.GetComponentsInAllChildren<MeshRenderer>();
            HashSet<MeshRenderer> bodyParts = new();
            
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                //check if it's a colourable body part
                bool isCopy = meshRenderer.sharedMaterial.name.Replace("(Clone)", "").Equals(bodyMaterial.name);
                if (meshRenderer.sharedMaterial == bodyMaterial || isCopy)
                    bodyParts.Add(meshRenderer);
            }
            
            colourableBodyParts = bodyParts.ToArray();
        }

        /// <summary>
        /// Ensure the material isn't the original, and create a clone so it isn't modified.
        /// </summary>
        private void EnsureMaterialIsCopy()
        {
            foreach (MeshRenderer meshRenderer in colourableBodyParts)
            {
                if (!meshRenderer.sharedMaterial.name.Contains("(Clone)"))
                    meshRenderer.sharedMaterial = Instantiate(meshRenderer.sharedMaterial);
            }
        }
        
    }
}
