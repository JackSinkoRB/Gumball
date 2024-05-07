using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Gumball
{
    public static class CorePartManager
    {
        
        /// <summary>
        /// The addressables address for the core parts data group.
        /// </summary>
        public const string CorePartsAssetLabel = "CorePart";
        
        private static readonly List<CorePart> allParts = new();
        private static readonly Dictionary<CorePart.PartType, CorePart[]> allPartsGrouped = new();
        private static readonly Dictionary<string, CorePart> partsMappedByID = new();

        public static ReadOnlyCollection<CorePart> AllParts => allParts.AsReadOnly();
        
        public static IEnumerator Initialise()
        {
            yield return FindParts();
        }

        public static CorePart GetPartByID(string ID)
        {
            if (ID == null)
                return null;
            
            if (!partsMappedByID.ContainsKey(ID))
                throw new NullReferenceException($"There is no core part with ID {ID}");
            
            return partsMappedByID[ID];
        }
        
        /// <returns>A collection of unlocked parts of type 'partType'.</returns>
        public static HashSet<CorePart> GetSpareParts(CorePart.PartType partType)
        {
            HashSet<CorePart> spareParts = new();

            if (!allPartsGrouped.ContainsKey(partType))
                return spareParts;
            
            foreach (CorePart part in allPartsGrouped[partType])
            {
                if (part.IsUnlocked && !part.IsAppliedToCar)
                    spareParts.Add(part);
            }
            
            return spareParts;
        }

        public static void InstallPartOnCurrentCar(CorePart.PartType type, CorePart part, int carIndex)
        {
            //if car has a part already installed, remove the reference to set it as a spare
            CorePart existingPart = PartModification.GetCorePart(carIndex, type);
            if (existingPart != null)
                existingPart.RemoveFromCar();

            //apply to car
            PartModification.SetCorePart(carIndex, type, part);
            
            //apply to part
            if (part != null)
                part.ApplyToCar(WarehouseManager.Instance.CurrentCar.CarIndex);
        }

        private static IEnumerator FindParts()
        {
            yield return AddressableUtils.LoadAssetsAsync(CorePartsAssetLabel, allParts, typeof(CorePart));

            GroupParts();
            CreateIDLookup();
        }

        private static void GroupParts()
        {
            Dictionary<CorePart.PartType, HashSet<CorePart>> grouped = new();
        
            //group
            foreach (CorePart part in allParts)
            {
                if (!grouped.ContainsKey(part.Type))
                    grouped[part.Type] = new HashSet<CorePart>();

                grouped[part.Type].Add(part);
            }

            //convert to arrays
            foreach (KeyValuePair<CorePart.PartType, HashSet<CorePart>> key in grouped)
            {
                allPartsGrouped[key.Key] = key.Value.ToArray();
            }
        }

        private static void CreateIDLookup()
        {
            foreach (CorePart corePart in allParts)
            {
                if (partsMappedByID.ContainsKey(corePart.ID))
                    Debug.LogWarning($"There are multiple core parts with ID {corePart.ID} ({corePart.name})");

                partsMappedByID[corePart.ID] = corePart;
            } 
        }
        
    }
}
