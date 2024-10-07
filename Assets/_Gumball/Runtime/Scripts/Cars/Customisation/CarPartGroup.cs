using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CarPartGroup : MonoBehaviour
    {
        
        [Header("Debugging")]
        [Tooltip("The car parts are retrieved and cached from under this object lazilly when required.")]
        [SerializeField, ReadOnly] private CarPart[] carPartsCached;
        [SerializeField, ReadOnly] private AICar carBelongsTo;
        [SerializeField, ReadOnly] private int currentPartIndex = -1;
        
        private string groupID => gameObject.name;
        private string savedPartIndexKey => $"{carBelongsTo.SaveKey}.{groupID}";

        public int CurrentPartIndex => currentPartIndex;

        public int SavedPartIndex => DataManager.Cars.Get(savedPartIndexKey, 0);
        
        public CarPart[] CarParts
        {
            get
            {
                if (carPartsCached == null || carPartsCached.Length == 0)
                {
                    HashSet<CarPart> parts = new();
                    foreach (Transform child in transform)
                    {
                        CarPart part = child.GetComponent<CarPart>();
                        if (part != null)
                            parts.Add(part);
                    }
                    carPartsCached = parts.ToArray();
                }
                
                return carPartsCached;
            }
        }

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
            
            //load the saved part, or set the first part active
            SetPartActive(SavedPartIndex);
        }

        public void SetPartActive(CarPart part)
        {
            SetPartActive(CarParts.IndexOfItem(part));    
        }
        
        public void SetPartActive(int index)
        {
            if (currentPartIndex == -1)
            {
                //disable all parts as it's not known if any part is active
                foreach (CarPart part in CarParts)
                    part.gameObject.SetActive(false);
            }
            else
            {
                //set the previous part inactive
                CarParts[currentPartIndex].gameObject.SetActive(false);
            }
            
            //update the current index
            currentPartIndex = index;
            
            //enable the new part
            CarParts[index].gameObject.SetActive(true);

            SaveData();
        }

        private void SaveData()
        {
            if (!carBelongsTo.IsPlayer)
                return; //non player cars don't get saved
            
            DataManager.Cars.Set(savedPartIndexKey, currentPartIndex);
        }

    }
}
