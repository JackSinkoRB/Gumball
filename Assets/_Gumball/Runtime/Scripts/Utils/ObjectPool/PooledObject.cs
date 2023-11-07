using System;
using UnityEngine;

/// <summary>
/// Not intended for manual use. This component is attached to objects automatically when setting up in the ObjectPool.
/// </summary>
public class PooledObject : MonoBehaviour
{

    [Tooltip("Should the object be pooled on disable?")]
    public bool PoolOnDisable = true;

    public bool IsPooled = false;

    public Action OnPoolAction = null;

    private void OnDisable()
    {
        if (PoolOnDisable)
        {
            gameObject.Pool();
        }
    }
    
}