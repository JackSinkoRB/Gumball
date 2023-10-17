using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CC
{
    public class Option_Picker : MonoBehaviour, ICustomizerUI
    {
        private CharacterCustomization customizer;
        private CC_UI_Util parentUI;

        public enum Type
        { Blendshape, Texture, Hair, Apparel };

        public Type CustomizationType;

        public CC_Property Property;
        public List<string> Objects = new List<string>();
        public int Slot = 0;

        public TextMeshProUGUI OptionText;

        private int navIndex = 0;
        private int optionsCount = 0;

        public void InitializeUIElement(CharacterCustomization customizerScript, CC_UI_Util ParentUI)
        {
            customizer = customizerScript;
            parentUI = ParentUI;

            RefreshUIElement();
        }

        public void RefreshUIElement()
        {
            switch (CustomizationType)
            {
                case Type.Blendshape:
                    {
                        optionsCount = Objects.Count;
                        updateOptionText();
                        for (int i = 0; i < customizer.StoredCharacterData.Blendshapes.Count; i++)
                        {
                            int j = Objects.FindIndex(t => t == customizer.StoredCharacterData.Blendshapes[i].propertyName);
                            if (j != -1)
                            {
                                navIndex = j;
                                updateOptionText();
                                break;
                            }
                        }
                        break;
                    }
                case Type.Texture:
                    {
                        optionsCount = Objects.Count;
                        int savedIndex = customizer.StoredCharacterData.TextureProperties.FindIndex(t => t.propertyName == Property.propertyName && t.materialIndex == Property.materialIndex && t.meshTag == Property.meshTag);
                        if (savedIndex != -1)
                        {
                            navIndex = Objects.FindIndex(t => t == customizer.StoredCharacterData.TextureProperties[savedIndex].stringValue);
                        }
                        else navIndex = 0;
                        updateOptionText();
                        break;
                    }
                case Type.Hair:
                    {
                        optionsCount = customizer.HairTables[Slot].Hairstyles.Count;
                        navIndex = customizer.HairTables[Slot].Hairstyles.FindIndex(t => t.Name == customizer.StoredCharacterData.HairNames[Slot]);
                        if (navIndex == -1) navIndex = 0;
                        updateOptionText();
                        break;
                    }
                case Type.Apparel:
                    {
                        optionsCount = customizer.ApparelTables[Slot].Items.Count;
                        navIndex = customizer.ApparelTables[Slot].Items.FindIndex(t => t.Name == customizer.StoredCharacterData.ApparelNames[Slot]);
                        if (navIndex == -1) navIndex = 0;
                        updateOptionText();
                        break;
                    }
            }
        }

        public void updateOptionText()
        {
            OptionText.SetText((navIndex + 1) + "/" + optionsCount);
        }

        public void setOption(int i)
        {
            navIndex = i;

            updateOptionText();

            switch (CustomizationType)
            {
                case Type.Blendshape:
                    {
                        foreach (string name in Objects)
                        {
                            customizer.setBlendshapeByName(name, 0);
                        }
                        customizer.setBlendshapeByName(Objects[i], 1); ;
                        break;
                    }
                case Type.Hair:
                    {
                        customizer.setHair(i, Slot);
                        break;
                    }
                case Type.Texture:
                    {
                        Property.stringValue = Objects[i];
                        customizer.setTextureProperty(Property, true);
                        break;
                    }
                case Type.Apparel:
                    {
                        customizer.setApparel(i, Slot);
                        break;
                    }
            }
        }

        public void navLeft()
        {
            setOption(navIndex == 0 ? optionsCount - 1 : navIndex - 1);
        }

        public void navRight()
        {
            setOption(navIndex == optionsCount - 1 ? 0 : navIndex + 1);
        }

        public void randomize()
        {
            setOption(Random.Range(0, optionsCount));
        }
    }
}