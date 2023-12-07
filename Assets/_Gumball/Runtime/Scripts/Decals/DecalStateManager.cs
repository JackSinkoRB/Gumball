using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class DecalStateManager
    {
        
        [Serializable]
        public abstract class StateChange
        {
            [SerializeField] protected LiveDecal liveDecal;
            [SerializeField] protected LiveDecal.LiveDecalData data;

            public LiveDecal LiveDecal => liveDecal;
            public LiveDecal.LiveDecalData Data => data;

            protected StateChange(LiveDecal liveDecal)
            {
                this.liveDecal = liveDecal;
                data = new LiveDecal.LiveDecalData(liveDecal);
            }
            
            public abstract void Undo();
            public abstract void Redo();
        }

        public class CreateStateChange : StateChange
        {
            public CreateStateChange(LiveDecal liveDecal) : base(liveDecal)
            {
            }
            
            public override void Undo()
            {
                DecalEditor.Instance.DisableLiveDecal(liveDecal);
            }

            public override void Redo()
            {
                liveDecal = DecalEditor.Instance.CreateLiveDecalFromData(data);
            }
        }
        
        public class ModifyStateChange : StateChange
        {
            private LiveDecal.LiveDecalData dataBeforeUndo;
            
            public ModifyStateChange(LiveDecal liveDecal) : base(liveDecal)
            {
            }
            
            public override void Undo()
            {
                dataBeforeUndo = new LiveDecal.LiveDecalData(liveDecal);
                
                //apply it's original data
                liveDecal.PopulateWithData(data);
            }

            public override void Redo()
            {
                liveDecal.PopulateWithData(dataBeforeUndo);
            }
        }
        
        public class DestroyStateChange : StateChange
        {
            public DestroyStateChange(LiveDecal liveDecal) : base(liveDecal)
            {
            }
            
            public override void Undo()
            {
                liveDecal = DecalEditor.Instance.CreateLiveDecalFromData(data);
            }

            public override void Redo()
            {
                DecalEditor.Instance.DisableLiveDecal(liveDecal);
            }
        }

        public static event Action onUndoStackChange;
        public static event Action onRedoStackChange;
        
        private static readonly List<StateChange> undoStack = new();
        private static readonly List<StateChange> redoStack = new();

        public static bool CanUndo => undoStack.Count > 0;
        public static bool CanRedo => redoStack.Count > 0;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialise()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public static void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
        
        /// <summary>
        /// Log the state of a decal so any changes after the logging can be undone.
        /// <remarks>Call this method BEFORE making changes to the decal.</remarks>
        /// </summary>
        public static void LogStateChange(StateChange stateChange)
        {
            //add to list, remove first element if reached max
            undoStack.Add(stateChange);
            onUndoStackChange?.Invoke();

            //clear the redo stack if something has changed
            if (redoStack.Count > 0)
            {
                redoStack.Clear();
                onRedoStackChange?.Invoke();
            }
            
            GlobalLoggers.DecalsLogger.Log($"Logged state change {stateChange.GetType().Name} (total = {undoStack.Count})");
        }

        public static void UndoLatestChange()
        {
            if (!CanUndo)
                return;

            StateChange latestChange = undoStack[^1];
            
            latestChange.Undo();
            undoStack.Remove(latestChange);
            onUndoStackChange?.Invoke();
            
            //add to redo stack
            redoStack.Add(latestChange);
            onRedoStackChange?.Invoke();
        }

        public static void RedoLatestUndo()
        {
            if (!CanRedo)
                return;
            
            StateChange latestChange = redoStack[^1];

            latestChange.Redo();
            redoStack.Remove(latestChange);
            onRedoStackChange?.Invoke();

            //add back to the undo stack
            undoStack.Add(latestChange);
            onUndoStackChange?.Invoke();
        }
        
    }
}
