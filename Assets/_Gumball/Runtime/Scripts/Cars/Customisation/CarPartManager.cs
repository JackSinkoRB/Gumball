using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gumball
{
    public class CarPartManager : MonoBehaviour
    {
        
        [Header("Debugging")]
        [Tooltip("The car part groups are retrieved and cached from under this object lazilly when required.")]
        [SerializeField] private CarPartGroup[] carPartGroupsCached;
        
        public CarPartGroup[] CarPartGroups
        {
            get
            {
                if (carPartGroupsCached == null || carPartGroupsCached.Length == 0)
                {
                    HashSet<CarPartGroup> partGroups = new();
                    foreach (Transform child in transform)
                    {
                        CarPartGroup partGroup = child.GetComponent<CarPartGroup>();
                        if (partGroup != null)
                            partGroups.Add(partGroup);
                    }
                    carPartGroupsCached = partGroups.ToArray();
                }
                
                return carPartGroupsCached;
            }
        }
        
    }
}
