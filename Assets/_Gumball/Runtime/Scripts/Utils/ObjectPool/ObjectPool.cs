using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using Gumball;
using Debug = UnityEngine.Debug;

public class ObjectPool : Singleton<ObjectPool> {

    //TODO: create an async object pool using addressables. Take inspiration from: https://thegamedev.guru/unity-addressables/pooling/

    [Tooltip("Assign poolable objects here")]
    [SerializeField] private PoolablePrefab[] poolablePrefabs;
    
    private Dictionary<string, PoolablePrefab> poolablePrefabsByName = new Dictionary<string, PoolablePrefab>();
    private Dictionary<string, List<PooledObject>> spareObjectsByName = new Dictionary<string, List<PooledObject>>();

    /// <summary>
    /// Retrieve a spare object from a prefab, otherwise create a new one.
    /// </summary>
    /// <param name="prefab">The prefab from the assets folder</param>
    /// <param name="assignToParent">The parent transform to assign the object too</param>
    /// <param name="poolOnDisable">Should the object be pooled when disabled in the hierarchy? If false, the object will need to be manually pooled.</param>
    public GameObject GetSpareOrCreate(GameObject prefab, Transform assignToParent = null,
        Vector3 worldPos = default, Quaternion rotation = default,
        bool poolOnDisable = true, Action onPool = null)
    {
        bool objectIsInScene = prefab.scene.name != null;
        if (objectIsInScene)
        {
            Debug.LogWarning(" '" + prefab.name + "' is an object in the scene. It is recommended to only use prefab objects from the assets folder, in case the object is modified or destroyed.");
        }

        string prefabName = prefab.name;
        if (!poolablePrefabsByName.ContainsKey(prefabName))
        {
            Debug.LogWarning("Could not find pooled prefab with name '" + prefabName + "'. Creating the PoolablePrefab now. Consider pre-pooling this object in the ObjectPool instance.");

            PoolablePrefab poolablePrefab = new PoolablePrefab(prefab);
            InitialisePoolablePrefab(poolablePrefab);
        }
        return GetSpareOrCreate(prefabName, assignToParent, worldPos, rotation, poolOnDisable, onPool);
    }

    /// <summary>
    /// Retrieve a spare object from a prefab name, otherwise create a new one.
    /// </summary>
    /// <remarks>It is recommended to get the object using the prefab object rather than the string name, in case the object name changes.</remarks>
    /// <param name="prefabName">The prefab name from the assets folder</param>
    /// <param name="assignToParent">The parent transform to assign the object too</param>
    /// <param name="poolOnDisable">Should the object be pooled when disabled in the hierarchy? If false, the object will need to be manually pooled.</param>
    public GameObject GetSpareOrCreate(string prefabName, Transform assignToParent = null,
        Vector3 worldPos = default,Quaternion rotation = default,
        bool poolOnDisable = true, Action onPool = null)
    {
        if (!poolablePrefabsByName.ContainsKey(prefabName))
        {
            Debug.LogError("Could not find pooled prefab with name '" + prefabName + "'. Is it set up in the ObjectPool instance?");
            return null;
        }

        PooledObject objectToGive = null;
        
        List<PooledObject> spares = spareObjectsByName.ContainsKey(prefabName) ? spareObjectsByName[prefabName] : new List<PooledObject>();
        if (spares.Count > 0)
        {
            GlobalLoggers.ObjectPoolLogger.Log("Taking object from '" + prefabName + "' pool. (" + spareObjectsByName[prefabName].Count + " spare)");
            
            //use first available spare
            objectToGive = spares[0];
            spares.Remove(objectToGive);
            
            objectToGive.transform.SetParent(assignToParent, false); //set parent?
        } else
        {
            GlobalLoggers.ObjectPoolLogger.Log("No available pooled items in '" + prefabName + "' pool. Creating a new one.");
            
            //instantiate a new one
            objectToGive = CreateObjectFromPrefab(GetPrefabByName(prefabName), assignToParent, worldPos, rotation);
        }
        
        objectToGive.gameObject.SetActive(true); //set object active
        objectToGive.PoolOnDisable = poolOnDisable; //all objects at this point will have a PooledObject attached, so no need to check
        objectToGive.IsPooled = false;
        objectToGive.OnPoolAction = onPool;
        
        return objectToGive.gameObject;
    }
    
    /// <summary>
    /// Discard the game object and add it to a pool.
    /// <remarks>Objects created by the pool will automatically be pooled on disable.</remarks>
    /// <seealso cref="PooledObject"/>
    /// </summary>
    /// <param name="gameObject">The game object to discard.</param>
    public void Pool(GameObject gameObject)
    {
        PooledObject pooledObject = gameObject.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            throw new NullReferenceException(gameObject.name + " isn't a PooledObject.");
        }
        Pool(pooledObject);
    }

    /// <summary>
    /// Discard the PooledObject's gameObject and add it to a pool.
    /// <remarks>Objects created by the pool will automatically be pooled on disable.</remarks>
    /// <seealso cref="PooledObject"/>
    /// </summary>
    /// <param name="pooledObject">The PooledObject to discard.</param>
    public void Pool(PooledObject pooledObject)
    {
        string prefabName = pooledObject.gameObject.name;
        if (!spareObjectsByName.ContainsKey(prefabName))
        {
            // create a new pool
            spareObjectsByName[prefabName] = new List<PooledObject>();
            GlobalLoggers.ObjectPoolLogger.Log("Creating new pool for '" + prefabName + "'.");
        }

        if (spareObjectsByName[prefabName].Contains(pooledObject))
            return; //the object is already pooled
        
        spareObjectsByName[prefabName].Add(pooledObject); //add to the spare object list

        //disable the object if not already
        if (pooledObject.gameObject.activeSelf)
            pooledObject.gameObject.SetActive(false);

        pooledObject.IsPooled = true;
        pooledObject.OnPoolAction?.Invoke();
        
        GlobalLoggers.ObjectPoolLogger.Log("Added " + pooledObject.gameObject.name + " to " + prefabName + " pool. (" + spareObjectsByName[prefabName].Count + " spare)");
    }

    public bool IsPooled(GameObject gameObject)
    {
        PooledObject pooledObject = gameObject.GetComponent<PooledObject>();
        return pooledObject != null && pooledObject.IsPooled;
    }

    protected override void Initialise()
    {
        base.Initialise();

        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (PoolablePrefab poolablePrefab in poolablePrefabs)
        {
            InitialisePoolablePrefab(poolablePrefab);
        }
        stopwatch.Stop();
        GlobalLoggers.LoadingLogger.Log("Took " + stopwatch.ElapsedMilliseconds + "ms to prewarm object pools.");
    }

    private void InitialisePoolablePrefab(PoolablePrefab poolablePrefab)
    {
        string prefabName = poolablePrefab.prefab.name;
        if (poolablePrefabsByName.ContainsKey(prefabName))
        {
            Debug.LogWarning("Multiple prefabs to pool found with the name '" + prefabName + "'.");
            return;
        }

        // assign name to poolablePrefabsByName
        poolablePrefabsByName[prefabName] = poolablePrefab;
        
        // pre-pool prefab with prePoolAmount
        for (int count = 0; count < poolablePrefab.prePoolAmount; count++)
        {
            //create the object and pool it
            PooledObject newObject = CreateObjectFromPrefab(poolablePrefab.prefab, transform); //just for pre-pooled objects, set parent to the ObjectPool instance
            Pool(newObject);
        }
    }

    /// <summary>
    /// Instantiates the prefab and adds a PooledObject component if it doesn't have one
    /// </summary>
    /// <returns>The attached PooledObject component</returns>
    private PooledObject CreateObjectFromPrefab(GameObject prefab, Transform assignToParent = null, Vector3 worldPos = default, Quaternion rotation = default)
    {
        GameObject newObject = Instantiate(prefab, worldPos, rotation, assignToParent);
        newObject.name = prefab.name;
            
        //ensure the new object has PooledObject attached
        PooledObject pooledObject = newObject.GetComponent<PooledObject>();
        if (pooledObject == null)
            pooledObject = newObject.AddComponent<PooledObject>();

        return pooledObject;
    }
    
    private GameObject GetPrefabByName(string prefabName)
    {
        return poolablePrefabsByName[prefabName].prefab;
    }
    
    [Serializable] 
    private class PoolablePrefab
    {
        public GameObject prefab;
        public ushort prePoolAmount;

        //if constructing at runtime, cannot set a pre-pool amount
        public PoolablePrefab(GameObject prefab)
        {
            this.prefab = prefab;
        }
    }
    
}

//helper methods
public static class GameObjectExtension {
    
    /// <summary>
    /// A helper method to pool an object.
    /// NOTE: This is the same as disabling the object if it has a PooledObject attached, as it is pooled on disable.
    /// </summary>
    public static void Pool(this GameObject gameObject)
    {
        if(ObjectPool.Instance != null)
            ObjectPool.Instance.Pool(gameObject);
    }

    /// <summary>
    /// Get a spare of this object from the pool or create a new one.
    /// NOTE: You must pass an object prefab that isn't in the scene
    /// </summary>
    public static GameObject GetSpareOrCreate(this GameObject prefab, Transform assignToParent = null,
        Vector3 position = default, Quaternion rotation = default,
        bool poolOnDisable = true, Action onPool = null)
        => ObjectPool.Instance.GetSpareOrCreate(prefab, assignToParent, position, rotation, poolOnDisable, onPool);

    /// <summary>
    /// Get a spare of this object from the pool or create a new one.
    /// NOTE: You must pass an object prefab that isn't in the scene
    /// <returns>The attached component of type T</returns>
    /// </summary>
    public static T GetSpareOrCreate<T>(this GameObject prefab, Transform assignToParent = null,
        Vector3 worldPos = default,Quaternion rotation = default,
        bool poolOnDisable = true, Action onPool = null)
        => prefab.GetSpareOrCreate(assignToParent, worldPos, rotation, poolOnDisable, onPool).GetComponent<T>();

    /// <summary>
    /// Gets whether a gameobject is pooled.
    /// </summary>
    public static bool IsPooled(this GameObject gameObject) => ObjectPool.Instance.IsPooled(gameObject);
    
}