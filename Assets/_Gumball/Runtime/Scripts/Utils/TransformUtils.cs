using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class TransformUtils
    {

        public static List<T> GetComponentsInAllChildren<T>(this Transform transform, List<T> existingComponents = null)
        {
            existingComponents ??= new List<T>();
            
            foreach (Transform child in transform)
            {
                T[] newComponents = child.GetComponents<T>();
    
                foreach (T component in newComponents)
                {
                    if (component != null)
                        existingComponents.Add(component);
                }
    
                GetComponentsInAllChildren<T>(child, existingComponents);
            }
    
            return existingComponents;
        }
        
    }
}
