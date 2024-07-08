using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// A generic solution responsible for picking the best state each frame.
    /// </summary>
    public abstract class DynamicState : MonoBehaviour
    {
        
        public event Action onSetCurrent;
        public event Action onEnd;

        [SerializeField] private int priority;
        [SerializeField] private bool checkToSetEachFrame;

        private DynamicStateManager manager;
        
        public int Priority => priority;
        public bool CheckToSetEachFrame => checkToSetEachFrame;

        public DynamicStateManager Manager {
            get
            {
                if (manager == null && !Application.isPlaying)
                    manager = transform.GetComponentInAllParents<DynamicStateManager>();
                return manager;
            } 
        }
        
        [Tooltip("The time passed (in seconds) since the state was set.")]
        [SerializeField, ReadOnly] protected float timeSinceSet;
        [Tooltip("The time passed (in seconds) since the state was last ended.")]
        [SerializeField, ReadOnly] protected float timeSinceEnded;

        public virtual void SetUp(DynamicStateManager manager)
        {
            this.manager = manager;
        }

        public virtual bool CanSetCurrent()
        {
            if (Manager.CurrentState == this) return false;
            if (!enabled || !gameObject.activeSelf) return false;
            if (Manager.CurrentState == null) return true;
            if (priority < Manager.CurrentState.Priority) return false;
            return true;
        }

        public virtual void OnSetCurrent()
        {
            timeSinceSet = 0;
            onSetCurrent?.Invoke();
        }

        /// <summary>
        /// Before performing the state it will run this check. If true, the state will be ended before it updates.
        /// </summary>
        public virtual bool CheckToEnd()
        {
            return false;
        }

        public virtual void OnEndState()
        {
            timeSinceEnded = 0;
            onEnd?.Invoke();
        }

        public virtual void Update()
        {
            timeSinceSet += Time.deltaTime;
            timeSinceEnded += Time.deltaTime;
        }
        
        public virtual void UpdateWhenCurrent()
        {
        }

        public virtual void LateUpdateWhenCurrent()
        {
        }
        
        public virtual void FixedUpdateWhenCurrent()
        {
            
        }
        
        /// <summary>
        /// Leaves this state and sets the current state to null.
        /// </summary>
        protected void EndState()
        {
            Manager.SetState(null);
        }
        
    }
}
