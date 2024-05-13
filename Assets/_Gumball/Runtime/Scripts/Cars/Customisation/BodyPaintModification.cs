using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class BodyPaintModification : MonoBehaviour
    {
        
        public enum PaintMode
        {
            SIMPLE,
            ADVANCED
        }
                
        //caching:
        public static readonly int BaseColorShaderID = Shader.PropertyToID("_BaseColor");
        public static readonly int SpecularShaderID = Shader.PropertyToID("_Specular");
        public static readonly int ClearCoatSmoothnessShaderID = Shader.PropertyToID("_ClearCoatSmoothness");
        public static readonly int ClearCoatShaderID = Shader.PropertyToID("_ClearCoat");
        public static readonly int SmoothnessShaderID = Shader.PropertyToID("_Smoothness");
        public static readonly int MetallicShaderID = Shader.PropertyToID("_Metallic");

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;
        [SerializeField, ReadOnly] private MeshRenderer[] colourableParts;

        private string saveKey => $"{carBelongsTo.SaveKey}.Paint.Body";

        public MeshRenderer[] ColourableParts => colourableParts;
        public PaintMode CurrentPaintMode => GetCurrentSwatchIndexInPresets() == -1 ? PaintMode.ADVANCED : PaintMode.SIMPLE;

        public ColourSwatchSerialized CurrentSwatch
        {
            get => DataManager.Cars.Get<ColourSwatchSerialized>($"{saveKey}.CurrentSwatch");
            set => DataManager.Cars.Set($"{saveKey}.CurrentSwatch", value);
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
            
            FindColourableParts();
            ApplySwatch(testSwatch);
        }
        
        [ButtonMethod]
        public void ClearTestSwatch()
        {
            FindColourableParts();
            foreach (MeshRenderer meshRenderer in colourableParts)
            {
                meshRenderer.sharedMaterial = GlobalPaintPresets.Instance.DefaultBodyMaterial;
            }
        }
#endif

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
            
            FindColourableParts();

            LoadFromSave();
        }

        public void ApplySwatch(ColourSwatch swatch)
        {
            ApplySwatch(swatch.Serialize());
        }
        
        public void ApplySwatch(ColourSwatchSerialized swatch)
        {
            EnsureMaterialIsCopy();
            
            foreach (MeshRenderer meshRenderer in colourableParts)
            {
                meshRenderer.sharedMaterial.SetColor(BaseColorShaderID, swatch.Color.ToColor());
                meshRenderer.sharedMaterial.SetColor(SpecularShaderID, swatch.Specular.ToColor());
                
                meshRenderer.sharedMaterial.SetFloat(SmoothnessShaderID, swatch.Smoothness);
                meshRenderer.sharedMaterial.SetFloat(ClearCoatShaderID, swatch.ClearCoat);
                meshRenderer.sharedMaterial.SetFloat(ClearCoatSmoothnessShaderID, swatch.ClearCoatSmoothness);
            }

            CurrentSwatch = swatch;
        }

        public void LoadFromSave()
        {
            if (!DataManager.Cars.HasKey($"{saveKey}.CurrentSwatch"))
            {
                ApplySwatch(GlobalPaintPresets.Instance.BodySwatchPresets[0]); //apply the default
                return;
            }

            ColourSwatchSerialized saveData = DataManager.Cars.Get<ColourSwatchSerialized>($"{saveKey}.CurrentSwatch");
            ApplySwatch(saveData);
        }

        /// <returns>Gets the index of the current swatch if it exists in the presets, else returns -1.</returns>
        public int GetCurrentSwatchIndexInPresets()
        {
            for (int index = 0; index < GlobalPaintPresets.Instance.BodySwatchPresets.Length; index++)
            {
                ColourSwatch colourSwatch = GlobalPaintPresets.Instance.BodySwatchPresets[index];
                if (CurrentSwatch.Equals(colourSwatch))
                    return index;
            }
            
            return -1;
        }

        /// <summary>
        /// Finds all the body parts and assigns a single material instance that can be modified.
        /// </summary>
        private void FindColourableParts()
        {
            List<MeshRenderer> meshRenderers = carBelongsTo.transform.GetComponentsInAllChildren<MeshRenderer>();
            HashSet<MeshRenderer> parts = new();
            
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (meshRenderer.sharedMaterial == null)
                    continue;
                
                //check if it's a colourable body part
                bool isCopy = meshRenderer.sharedMaterial.name.Replace("(Clone)", "").Equals(GlobalPaintPresets.Instance.DefaultBodyMaterial.name);
                if (meshRenderer.sharedMaterial == GlobalPaintPresets.Instance.DefaultBodyMaterial || isCopy)
                    parts.Add(meshRenderer);
            }
            
            colourableParts = parts.ToArray();
        }

        /// <summary>
        /// Ensure the material isn't the original, and create a clone so it isn't modified.
        /// </summary>
        private void EnsureMaterialIsCopy()
        {
            foreach (MeshRenderer meshRenderer in colourableParts)
            {
                if (!meshRenderer.sharedMaterial.name.Contains("(Clone)"))
                    meshRenderer.sharedMaterial = Instantiate(meshRenderer.sharedMaterial);
            }
        }
        
    }
}
