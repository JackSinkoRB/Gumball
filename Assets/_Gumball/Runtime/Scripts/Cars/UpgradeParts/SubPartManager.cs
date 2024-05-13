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
        
        public static IEnumerator Initialise()
        {
            yield return FindParts();
        }
        
        private static IEnumerator FindParts()
        {
            yield return AddressableUtils.LoadAssetsAsync(SubPartsAssetLabel, allParts, typeof(SubPart));
        }
        
    }
}
