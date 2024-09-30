using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(WheelMesh))]
    public class WheelPaintModification : MonoBehaviour
    {

        //caching:
        public static readonly int BaseColorShaderID = Shader.PropertyToID("_BaseColor");
        public static readonly int SpecularShaderID = Shader.PropertyToID("_Specular");
        public static readonly int ClearCoatSmoothnessShaderID = Shader.PropertyToID("_ClearCoatSmoothness");
        public static readonly int ClearCoatShaderID = Shader.PropertyToID("_ClearCoat");
        public static readonly int SmoothnessShaderID = Shader.PropertyToID("_Smoothness");
        
        public enum PaintMode
        {
            SIMPLE,
            ADVANCED
        }

        [SerializeField] private int defaultSwatchIndex;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AICar carBelongsTo;
        [SerializeField, ReadOnly] private MeshRenderer[] colourableParts;

        private int wheelIndex;
        private WheelMesh wheelMesh => GetComponent<WheelMesh>();
        
        private string saveKey => $"{carBelongsTo.SaveKey}.Paint.Wheel.{wheelIndex}";
        
        public PaintMode CurrentPaintMode => GetSavedSwatchIndexInPresets() == -1 ? PaintMode.ADVANCED : PaintMode.SIMPLE;
        public MeshRenderer[] ColourableParts => colourableParts;
        public int DefaultSwatchIndex => defaultSwatchIndex;
        
        public ColourSwatchSerialized SavedSwatch
        {
            get
            {
                if (!carBelongsTo.IsPlayer)
                    throw new InvalidOperationException("Cannot get save value for non-player car.");
                return DataManager.Cars.Get<ColourSwatchSerialized>($"{saveKey}.CurrentSwatch");
            }
            set
            {
                if (carBelongsTo.IsPlayer)
                    DataManager.Cars.Set($"{saveKey}.CurrentSwatch", value);
            }
        }

        public int SavedSelectedPresetIndex
        {
            get
            {
                if (!carBelongsTo.IsPlayer)
                    throw new InvalidOperationException("Cannot get save value for non-player car.");
                return DataManager.Cars.Get($"{saveKey}.SelectedPreset", defaultSwatchIndex);
            }
            set
            {
                if (carBelongsTo.IsPlayer)
                    DataManager.Cars.Set($"{saveKey}.SelectedPreset", value);
            }
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
                meshRenderer.sharedMaterial = GlobalPaintPresets.Instance.DefaultWheelMaterial;
                ColourSwatch defaultSwatch = GlobalPaintPresets.Instance.WheelSwatchPresets[defaultSwatchIndex];
                ApplySwatch(defaultSwatch);
            }
        }
#endif
        
        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
            
            wheelIndex = carBelongsTo.AllWheelMeshes.IndexOfItem(wheelMesh);
            FindColourableParts();

            if (carBelongsTo.IsPlayer)
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

            if (carBelongsTo.IsPlayer)
                SavedSwatch = swatch;
        }

        public void LoadFromSave()
        {
            if (!DataManager.Cars.HasKey($"{saveKey}.CurrentSwatch"))
            {
                ApplySwatch(GlobalPaintPresets.Instance.WheelSwatchPresets[defaultSwatchIndex]); //apply the default
                return;
            }

            ColourSwatchSerialized saveData = DataManager.Cars.Get<ColourSwatchSerialized>($"{saveKey}.CurrentSwatch");
            ApplySwatch(saveData);
        }
        
        /// <returns>Gets the index of the current swatch if it exists in the presets, else returns -1.</returns>
        public int GetSavedSwatchIndexInPresets()
        {
            for (int index = 0; index < GlobalPaintPresets.Instance.WheelSwatchPresets.Length; index++)
            {
                ColourSwatch colourSwatch = GlobalPaintPresets.Instance.WheelSwatchPresets[index];
                if (SavedSwatch.Equals(colourSwatch))
                    return index;
            }
            
            return -1;
        }
        
        /// <summary>
        /// Finds all the body parts and assigns a single material instance that can be modified.
        /// </summary>
        private void FindColourableParts()
        {
            List<MeshRenderer> meshRenderers = wheelMesh.Rim.transform.GetComponentsInAllChildren<MeshRenderer>();
            HashSet<MeshRenderer> parts = new();
            
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (meshRenderer.sharedMaterial == null)
                    continue;
                
                parts.Add(meshRenderer);
            }
            
            colourableParts = parts.ToArray();
        }

        public void SetMaterialType(PaintMaterial.Type type)
        {
            //save to file
            ColourSwatchSerialized swatch = SavedSwatch;
            swatch.SetMaterialType(type);
            SavedSwatch = swatch;
            
            //update visuals
            ApplySwatch(SavedSwatch);
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
