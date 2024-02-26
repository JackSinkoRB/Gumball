using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// A generic solution responsible for picking the best state each frame.
    /// </summary>
    public abstract class DynamicStateManager : MonoBehaviour
    {

        public event Action<DynamicState> onStateChange;
        
        [SerializeField] private Logger logger;
        
        [Space(5)]
        [Tooltip("A set of all the states attached to this manager.")]
        [SerializeField, ReadOnly] private SerializedDictionary<Type, DynamicState> allStates = new();
        [SerializeField, ReadOnly] private List<DynamicState> statesToCheckToSetEachFrame = new();
        [SerializeField, ReadOnly] private DynamicState currentState;

        private bool isInitialised;
        
        public Logger Logger => logger;
        public DynamicState CurrentState => currentState;

        private void OnEnable()
        {
            Logger.Log(logger, gameObject.name + " was enabled.");

            if (!isInitialised)
                Initialise();
        }

        private void Initialise()
        {
            isInitialised = true;
            FindStates();
        }
        
        private void OnDisable()
        {
            Logger.Log(logger, gameObject.name + " was disabled.");

            EndCurrentState();
        }

        private void Update()
        {
            ChooseState();
            if (currentState != null)
                currentState.UpdateWhenCurrent();
        }

        private void LateUpdate()
        {
            if(currentState != null)
                currentState.LateUpdateWhenCurrent();
        }

        private void FixedUpdate()
        {
            if(currentState != null)
                currentState.FixedUpdateWhenCurrent();
        }

        public void SetState(DynamicState state)
        {
            if (currentState == state)
                return; //already this state

            EndCurrentState();
            currentState = state;
            
            if (state != null)
                state.OnSetCurrent();

            onStateChange?.Invoke(state);
                
            Logger.Log(logger, gameObject.name + " set state to " + (state == null ? "none" : state.name) + ".");
        }

        public T GetState<T>() where T : DynamicState
        {
            return (T)allStates[typeof(T)];
        }
        
        private void ChooseState()
        {
            if (currentState != null && currentState.CheckToEnd())
                EndCurrentState();
            
            foreach (DynamicState state in statesToCheckToSetEachFrame) {
                if (state.CanSetCurrent()) {
                    SetState(state);
                    return;
                }
            }
        }

        private void EndCurrentState()
        {
            if (currentState == null)
                return;
        
            Logger.Log(logger, "Ending " + currentState.gameObject.name + ".");

            currentState.OnEndState();
            currentState = null;
        }
        
        /// <summary>
        /// Find all states under this manager.
        /// </summary>
        private void FindStates()
        {
            foreach(DynamicState state in transform.GetComponentsInAllChildren<DynamicState>())
            {
                state.SetUp(this);
                allStates[state.GetType()] = state;
                
                Logger.Log(logger, gameObject.name + " found state '" + state.name + "'.");

                if(state.CheckToSetEachFrame)
                    statesToCheckToSetEachFrame.Add(state);
            }
            
            //sort list by priority
            statesToCheckToSetEachFrame = statesToCheckToSetEachFrame.OrderByDescending(state => state.Priority).ToList();
        }
    }
}
