using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class ColorableBlendshapeCosmetic : BlendshapeCosmetic
    {
        
        [SerializeField] private ColorableCosmeticOption colorable;

        [Foldout("Debugging"), SerializeField, ReadOnly]
        private int currentColorIndex = -1;

        private string colorSaveKey => $"{saveKey}.SelectedColorIndex";

        public ColorableCosmeticOption Colorable => colorable;
        public int CurrentColorIndex => currentColorIndex;
        
        protected override void OnApplyCosmetic(int index)
        {
            base.OnApplyCosmetic(index);

            ApplyDefaultColor();
        }
        
        public override void Save()
        {
            base.Save();
            
            DataManager.Avatar.Set(colorSaveKey, currentColorIndex);
        }
        
        public int GetSavedColorIndex()
        {
            return DataManager.Avatar.Get(colorSaveKey, -1);
        }
        
        private void ApplyDefaultColor()
        {
            if (currentColorIndex == -1)
                currentColorIndex = GetSavedColorIndex();

            if (currentColorIndex == -1
                || currentColorIndex >= colorable.Colors.Length)
                currentColorIndex = colorable.DefaultColorIndex;

            ApplyColor(currentColorIndex);
        }

        public void ApplyColor(int index)
        {
            currentColorIndex = index;

            foreach (string colorProperty in colorable.ColorMaterialProperties)
            {
                foreach (Material material in avatarBelongsTo.CurrentBody.GetMaterialsWithProperty(colorProperty))
                {
                    if (colorable.CanIgnoreMaterial(material))
                        continue;
                    
                    material.SetColor(colorProperty, colorable.Colors[index]);
                }
            }
        }
        
    }
}
