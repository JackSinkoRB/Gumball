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

        public static ReadOnlyCollection<CorePart> AllParts => allParts.AsReadOnly();
        
        public static IEnumerator Initialise()
        {
            yield return FindParts();
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

        private static IEnumerator FindParts()
        {
            yield return AddressableUtils.LoadAssetsAsync(CorePartsAssetLabel, allParts, typeof(CorePart));
            
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
        
    }
}
