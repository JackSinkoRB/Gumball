using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// Singleton class that will create an instance if one doesn't exist.
    /// </summary>
    /// <remarks>You can add [ExecuteInEditMode] if you need the instance in edit mode (eg. for edit mode tests).</remarks>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    //create an instance
                    instance = new GameObject(typeof(T).FullName).AddComponent<T>();
                    DontDestroyOnLoad(instance.gameObject);
                }

                return instance;
            }
        }

    }
}