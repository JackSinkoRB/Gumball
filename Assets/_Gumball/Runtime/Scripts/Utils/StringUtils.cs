using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class StringUtils
    {

        /// <summary>
        /// Checks if the string contains any of the strings in the collection.
        /// </summary>
        public static bool ContainsAny(this string stringToCheck, IEnumerable stringsToMatch)
        {
            foreach (string stringToMatch in stringsToMatch)
            {
                if (stringToCheck.Contains(stringToMatch))
                    return true;
            }

            return false;
        }
        
    }
}
