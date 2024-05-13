using System;
using System.Collections;
using System.Collections.Generic;
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

        public static IEnumerator Initialise()
        {
            yield return FindParts();

            CreateIDLookup();
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
            yield return AddressableUtils.LoadAssetsAsync(SubPartsAssetLabel, allParts, typeof(SubPart));
        }
        
        private static void CreateIDLookup()
        {
            foreach (SubPart subPart in allParts)
            {
                if (partsMappedByID.ContainsKey(subPart.ID))
                    Debug.LogWarning($"There are multiple sub parts with ID {subPart.ID} ({subPart.name})");

                partsMappedByID[subPart.ID] = subPart;
            } 
        }
        
    }
}
