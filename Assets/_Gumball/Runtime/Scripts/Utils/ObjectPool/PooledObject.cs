using System;
using MyBox;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// Not intended for manual use. This component is attached to objects automatically when setting up in the ObjectPool.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {

        [SerializeField, ReadOnly] private string prefabName;
        [SerializeField, ReadOnly] private bool isPooled;
        [Tooltip("Should the object be pooled on disable?")]
        [SerializeField, ReadOnly] private bool poolOnDisable = true;

        private Action onPoolAction;
        
        public string PrefabName => prefabName;
        public bool IsPooled => isPooled;
        public bool PoolOnDisable => poolOnDisable;
        
        public void Initialise(string prefabName, bool poolOnDisable, Action onPoolAction)
        {
            this.prefabName = prefabName;
            this.poolOnDisable = poolOnDisable;
            this.onPoolAction = onPoolAction;
            
            SetPooled(false);
        }
        
        public void SetPooled(bool isPooled)
        {
            this.isPooled = isPooled;
            
            if (isPooled)
                onPoolAction?.Invoke();
        }
        
        private void OnDisable()
        {
            if (PoolOnDisable)
            {
                gameObject.Pool();
            }
        }

        private void OnDestroy()
        {
            if (gameObject.IsPooled())
            {
                gameObject.RemoveFromPool();
            }
        }
    
    }   
}