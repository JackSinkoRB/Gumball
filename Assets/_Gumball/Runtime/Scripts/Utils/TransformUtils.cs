using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public static class TransformUtils
    {

        public static Transform GetActiveChild(this Transform transform, int index)
        {
            int count = 0;
            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf)
                    continue;
                
                if (count == index)
                    return child;
                
                count++;
            }

            return null;
        }
        
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
