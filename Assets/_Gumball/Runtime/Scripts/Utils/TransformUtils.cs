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

        public static T FindComponentInParents<T>(this Transform transform) where T : MonoBehaviour
        {
            Transform parent = transform;
            while (parent != null)
            {
                T componentOnParent = parent.GetComponent<T>();
                if (componentOnParent != null)
                    return componentOnParent;

                parent = parent.parent;
            }

            return null; //no component
        }

    }
}
