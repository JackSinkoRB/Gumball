using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class GameObjectExtensions
    {
        public static T GetComponent<T>(this GameObject gameObject, bool addIfNull) where T : Component
        {
            var value = gameObject.GetComponent<T>();
            if (value == null && addIfNull)
                value = gameObject.AddComponent<T>();
            return value;
        }

        public static T GetComponent<T>(this Component component, bool addIfNull) where T : Component
        {
            return component.gameObject.GetComponent<T>(addIfNull);
        }
    }
}
