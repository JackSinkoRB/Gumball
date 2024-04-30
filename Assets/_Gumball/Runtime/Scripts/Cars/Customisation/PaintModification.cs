using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class ColourModification : MonoBehaviour
    {
        
        //caching:
        private static readonly int Emission = Shader.PropertyToID("_Emission");
        private static readonly int ClearCoatSmoothness = Shader.PropertyToID("_ClearCoatSmoothness");
        private static readonly int ClearCoat = Shader.PropertyToID("_ClearCoat");
        private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
        private static readonly int Mettalic = Shader.PropertyToID("_Mettalic");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private ColourSwatch[] swatchPresets;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;
        [SerializeField, ReadOnly] private MeshRenderer[] colourableBodyParts;

        private string saveKey => $"{carBelongsTo.SaveKey}.Paint";
        public ColourSwatch[] SwatchPresets => swatchPresets;

        private ColourSwatchSerialized currentBodyColour
        {
            get => DataManager.Cars.Get<ColourSwatchSerialized>($"{saveKey}.BodyColour");
            set => DataManager.Cars.Set($"{saveKey}.BodyColour", value);
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
                meshRenderer.sharedMaterial.SetColor(BaseColor, swatch.Color.ToColor());
                meshRenderer.sharedMaterial.SetColor(Emission, swatch.Emission.ToColor());
                
                meshRenderer.sharedMaterial.SetFloat(Mettalic, swatch.Metallic);
                meshRenderer.sharedMaterial.SetFloat(Smoothness, swatch.Smoothness);
                meshRenderer.sharedMaterial.SetFloat(ClearCoat, swatch.ClearCoat);
                meshRenderer.sharedMaterial.SetFloat(ClearCoatSmoothness, swatch.ClearCoatSmoothness);
            }

            currentBodyColour = swatch;
        }

        /// <summary>
        /// Gets the index of the current swatch if it exists in the presets, else returns the default swatch (0).
        /// </summary>
        public int GetCurrentSwatchIndexInPresets()
        {
            for (int index = 0; index < swatchPresets.Length; index++)
            {
                ColourSwatch colourSwatch = swatchPresets[index];
                if (currentBodyColour.Equals(colourSwatch))
                    return index;
            }
            
            return 0;
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
