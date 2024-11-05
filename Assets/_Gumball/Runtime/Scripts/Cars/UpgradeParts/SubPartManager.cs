using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Gumball
{
    public static class SubPartManager
    {
    
        /// <summary>
        /// The addressables label for the sub parts data group.
        /// </summary>
        public const string SubPartsAssetLabel = "SubPart";
        
        private static readonly List<SubPart> allParts = new();
        private static readonly Dictionary<string, SubPart> partsMappedByID = new();
        private static readonly Dictionary<SubPart.SubPartType, SubPart[]> allPartsGrouped = new();
        
        public static ReadOnlyCollection<SubPart> AllParts => allParts.AsReadOnly();
        
        private static bool isInitialised;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            isInitialised = false;
        }

        public static IEnumerator Initialise()
        {
            if (isInitialised)
                yield break;

            isInitialised = true;
            
            yield return FindParts();

            CreateIDLookup();
            GroupParts();
        }

        public static SubPart GetSpareSubPart(SubPart.SubPartType type, SubPart.SubPartRarity rarity)
        {
            if (!allPartsGrouped.ContainsKey(type))
                return null;
            
            foreach (SubPart subPart in allPartsGrouped[type])
            {
                if (subPart.Rarity == rarity && subPart.IsUnlocked && !subPart.IsAppliedToCorePart)
                    return subPart;
            }

            return null;
        }
        
        public static HashSet<SubPart> GetSpareSubParts(SubPart.SubPartType type, SubPart.SubPartRarity rarity)
        {
            HashSet<SubPart> spares = new();
            
            if (!allPartsGrouped.ContainsKey(type))
                return spares;
            
            foreach (SubPart subPart in allPartsGrouped[type])
            {
                if (subPart.Rarity == rarity && subPart.IsUnlocked && !subPart.IsAppliedToCorePart)
                    spares.Add(subPart);
            }

            return spares;
        }
        
        public static HashSet<SubPart> GetSubParts(SubPart.SubPartType type, SubPart.SubPartRarity rarity)
        {
            HashSet<SubPart> parts = new();
            
            if (!allPartsGrouped.ContainsKey(type))
                return parts;
            
            foreach (SubPart subPart in allPartsGrouped[type])
            {
                if (subPart.Rarity == rarity)
                    parts.Add(subPart);
            }

            return parts;
        }
        
        public static SubPart GetPartByID(string ID)
        {
            if (ID == null)
                return null;
            
            if (!partsMappedByID.ContainsKey(ID))
                throw new NullReferenceException($"There is no sub part with ID {ID}");
            
            return partsMappedByID[ID];
        }
        
        private static IEnumerator FindParts()
        {
            allParts.Clear();
            
            yield return AddressableUtils.LoadAssetsAsync(SubPartsAssetLabel, allParts);
        }
        
        private static void GroupParts()
        {
            allPartsGrouped.Clear();
            
            Dictionary<SubPart.SubPartType, HashSet<SubPart>> grouped = new();
        
            //group
            foreach (SubPart part in allParts)
            {
                if (!grouped.ContainsKey(part.Type))
                    grouped[part.Type] = new HashSet<SubPart>();

                grouped[part.Type].Add(part);
            }

            //convert to arrays
            foreach (KeyValuePair<SubPart.SubPartType, HashSet<SubPart>> key in grouped)
            {
                allPartsGrouped[key.Key] = key.Value.ToArray();
            }
        }
        
        private static void CreateIDLookup()
        {
            partsMappedByID.Clear();
            
            foreach (SubPart subPart in allParts)
            {
                if (partsMappedByID.ContainsKey(subPart.ID))
                    Debug.LogWarning($"There are multiple sub parts with ID {subPart.ID} ({subPart.name})");

                partsMappedByID[subPart.ID] = subPart;
            } 
        }
        
    }
}
