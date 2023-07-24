using System;
using UnityEngine;
using MyBox;

/// <summary>
/// Generic singleton class.
/// </summary>
/// <remarks>You can add [ExecuteInEditMode] if you need the instance in edit mode (eg. for edit mode tests).</remarks>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                    Debug.LogWarning($"Tried lazilly accessing {typeof(T)}, but it couldn't be found.");
                if (Application.isPlaying)
                    Debug.LogWarning($"Lazilly accessing {instance.name} singleton, as it is not yet initialised.");
            }
            
            return instance;
        }
    }

    [Foldout("Singleton"), InitializationField, SerializeField]
    protected bool dontDestroy = true;
    
    protected void Awake()
    {
        if (instance != null)
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
        instance = this as T;
        
        if (dontDestroy)
        {
            transform.parent = null; //can't be a child
            DontDestroyOnLoad(gameObject);
        }
    }
    
}