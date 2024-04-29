using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class PaintWorkshopMenu : WorkshopSubMenu
    {

        [SerializeField] private MagneticScroll bodyColourSwatchMagneticScroll;

        public override void Show()
        {
            base.Show();

            ColourModification colourModification = WarehouseManager.Instance.CurrentCar.ColourModification;
            PopulateScroll(colourModification);
        }

        private void PopulateScroll(ColourModification colourModification)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            
            foreach (ColourSwatch colourSwatch in colourModification.SwatchPresets)
            {
                ScrollItem scrollItem = new ScrollItem();
                
                scrollItem.onLoad += () =>
                {
                    ColourScrollIcon partsScrollIcon = (ColourScrollIcon)scrollItem.CurrentIcon;
                    partsScrollIcon.ImageComponent.color = colourSwatch.Color;
                    partsScrollIcon.SecondaryColour.color = colourSwatch.Emission;
                };

                scrollItem.onSelect += () =>
                {
                    colourModification.ApplySwatch(colourSwatch);
                };

                scrollItems.Add(scrollItem);
            }

            bodyColourSwatchMagneticScroll.SetItems(scrollItems, colourModification.GetCurrentSwatchIndexInPresets());
        }
        
    }
}
