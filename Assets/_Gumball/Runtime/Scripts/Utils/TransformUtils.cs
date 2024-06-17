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

        public static T GetComponentInAllParents<T>(this Transform transform) where T : MonoBehaviour
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
        
        /// <summary>
        /// Checks if the transform has the component T enabled, or if any of the children have it enabled.
        /// </summary>
        public static bool HasActiveComponentsInChildren<T>(this Transform transform)
        {
            T self = transform.GetComponent<T>();
            if (self != null && (self is not MonoBehaviour selfMonoBehaviour || selfMonoBehaviour.enabled))
                return true;

            foreach (T child in transform.GetComponentsInAllChildren<T>())
            {
                if (child != null && (child is not MonoBehaviour childMonoBehaviour || childMonoBehaviour.enabled))
                    return true;
            }

            return false;
        }

        public static Transform FindChildByName(this Transform parent, string name)
        {
            if (parent.name == name)
                return parent.transform;
            
            foreach (Transform child in parent)
            {
                Transform childOfChild = child.FindChildByName(name);
                if (childOfChild != null)
                    return childOfChild;
            }
            
            return null;
        }
        
    }
}
