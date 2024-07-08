using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class MultipleEnumUtils
    {

        public static List<T> GetSelectedValues<T>(T enumProperty) where T : Enum
        {
            List<T> selectedElements = new List<T>();
            Array enumValues = Enum.GetValues(typeof(T));
            for (int i = 0; i < enumValues.Length; i++)
            {
                int layer = 1 << i;
                if (((int)(object)enumProperty & layer) != 0)
                {
                    selectedElements.Add((T)enumValues.GetValue(i));
                }
            }

            return selectedElements;
        }

    }
}