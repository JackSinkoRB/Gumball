using System;
using UnityEngine;
using MyBox;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    
    public static T Instance { get; private set; }
    
    [Foldout("Singleton"), InitializationField, SerializeField]
    protected bool dontDestroy = true;

    protected void Awake()
    {
        if (Instance != null)
        {
            //instance already exists, so just remove this new one
            gameObject.SetActive(false); //set inactive so it doesn't trigger colliders etc.
            Destroy(gameObject);
            return;
        }

        Initialise();
    }

    protected virtual void Initialise()
    {
        Instance = this as T;
        
        if (dontDestroy)
        {
            transform.parent = null; //can't be a child
            DontDestroyOnLoad(gameObject);
        }
    }

}