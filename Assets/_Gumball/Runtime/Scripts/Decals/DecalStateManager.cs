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
            public abstract void Undo();
            public abstract void Redo();
        }

        [Serializable]
        public abstract class SingleDecalStateChange : StateChange
        {
            [SerializeField] protected LiveDecal liveDecal;
            [SerializeField] protected LiveDecal.LiveDecalData data;

            public LiveDecal LiveDecal => liveDecal;
            public LiveDecal.LiveDecalData Data => data;

            protected SingleDecalStateChange(LiveDecal liveDecal)
            {
                this.liveDecal = liveDecal;
                data = new LiveDecal.LiveDecalData(liveDecal);
            }
        }
        
        [Serializable]
        public class SwitchPrioritiesStateChange : StateChange
        {
            [SerializeField] protected LiveDecal liveDecal1;
            [SerializeField] protected LiveDecal liveDecal2;
            [SerializeField] protected int priority1;
            [SerializeField] protected int priority2;
            
            public LiveDecal LiveDecal1 => liveDecal1;
            public LiveDecal LiveDecal2 => liveDecal1;
            public int Priority1 => priority1;
            public int Priority2 => priority2;
            
            public SwitchPrioritiesStateChange(LiveDecal liveDecal1, LiveDecal liveDecal2)
            {
                this.liveDecal1 = liveDecal1;
                this.liveDecal2 = liveDecal2;
                priority1 = liveDecal1.Priority;
                priority2 = liveDecal2.Priority;
            }
            
            public override void Undo()
            {
                //flip the priorities
                liveDecal1.SetPriority(priority1);
                liveDecal2.SetPriority(priority2);
            
                //priorities have changed, make sure to reorder the list
                DecalEditor.Instance.OrderDecalsListByPriority();
                
                DecalEditor.Instance.SelectLiveDecal(liveDecal1);
            }

            public override void Redo()
            {
                //flip the priorities
                liveDecal1.SetPriority(priority2);
                liveDecal2.SetPriority(priority1);
            
                //priorities have changed, make sure to reorder the list
                DecalEditor.Instance.OrderDecalsListByPriority();
                
                DecalEditor.Instance.SelectLiveDecal(liveDecal1);
            }
        }

        public class CreateStateChange : SingleDecalStateChange
        {
            public CreateStateChange(LiveDecal liveDecal) : base(liveDecal)
            {
            }
            
            public override void Undo()
            {
                DecalEditor.Instance.DisableLiveDecal(liveDecal);
                DecalEditor.Instance.DeselectLiveDecal();
            }

            public override void Redo()
            {
                liveDecal = DecalEditor.Instance.CreateLiveDecalFromData(data);
                DecalEditor.Instance.SelectLiveDecal(liveDecal);
            }
        }
        
        public class ModifyStateChange : SingleDecalStateChange
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
                
                DecalEditor.Instance.SelectLiveDecal(liveDecal);
            }

            public override void Redo()
            {
                liveDecal.PopulateWithData(dataBeforeUndo);
                
                DecalEditor.Instance.SelectLiveDecal(liveDecal);
            }
        }
        
        public class ColorStateChange : SingleDecalStateChange
        {
            private int colorIndexBeforeUndo;
            
            public ColorStateChange(LiveDecal liveDecal) : base(liveDecal)
            {
            }
            
            public override void Undo()
            {
                colorIndexBeforeUndo = liveDecal.ColorIndex;
                
                liveDecal.SetColorFromIndex(data.ColorIndex);
                
                DecalEditor.Instance.SelectLiveDecal(liveDecal);
            }

            public override void Redo()
            {
                liveDecal.SetColorFromIndex(colorIndexBeforeUndo);
                
                DecalEditor.Instance.SelectLiveDecal(liveDecal);
            }
        }
        
        public class DestroyStateChange : SingleDecalStateChange
        {
            public DestroyStateChange(LiveDecal liveDecal) : base(liveDecal)
            {
            }
            
            public override void Undo()
            {
                liveDecal = DecalEditor.Instance.CreateLiveDecalFromData(data);
                DecalEditor.Instance.SelectLiveDecal(liveDecal);
            }

            public override void Redo()
            {
                DecalEditor.Instance.DisableLiveDecal(liveDecal);
                DecalEditor.Instance.DeselectLiveDecal();
            }
        }

        public static event Action onUndoStackChange;
        public static event Action onRedoStackChange;
        
        private static readonly List<StateChange> undoStack = new();
        private static readonly List<StateChange> redoStack = new();

        public static bool CanUndo => undoStack.Count > 0;
        public static bool CanRedo => redoStack.Count > 0;

        public static StateChange NextUndoState => undoStack.Count > 0 ? undoStack[^1] : null;

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
