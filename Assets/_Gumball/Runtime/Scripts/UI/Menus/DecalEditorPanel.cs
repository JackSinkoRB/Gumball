using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalEditorPanel : AnimatedPanel
    {

        [Header("Decal editor panel")]
        [SerializeField] private DecalLayerSelector layerSelector;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private Button trashButton;
        
        public DecalLayerSelector LayerSelector => layerSelector;
        public Button TrashButton => trashButton;
        public Button UndoButton => undoButton;
        public Button RedoButton => redoButton;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            DecalEditor.Instance.onSelectLiveDecal += OnSelectDecal;
            DecalEditor.Instance.onDeselectLiveDecal += OnDeselectDecal;
            DecalStateManager.onUndoStackChange += OnUndoStackChange;
            DecalStateManager.onRedoStackChange += OnRedoStackChange;

            //starting a new session, so the stacks will have changed
            OnUndoStackChange();
            OnRedoStackChange();
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (DecalEditor.ExistsRuntime)
            {
                DecalEditor.Instance.onSelectLiveDecal -= OnSelectDecal;
                DecalEditor.Instance.onDeselectLiveDecal -= OnDeselectDecal;
            }
            DecalStateManager.onUndoStackChange -= OnUndoStackChange;
            DecalStateManager.onRedoStackChange -= OnRedoStackChange;
        }

        public void OnClickBackButton()
        {
            DecalEditor.Instance.EndSession();
            MainSceneManager.LoadMainScene();
        }
        
        public void OnClickTrashButton()
        {
            DecalStateManager.LogStateChange(new DecalStateManager.DestroyStateChange(DecalEditor.Instance.CurrentSelected));
            DecalEditor.Instance.DisableLiveDecal(DecalEditor.Instance.CurrentSelected);
        }

        public void OnClickUndoButton()
        {
            DecalStateManager.UndoLatestChange();
        }

        public void OnClickRedoButton()
        {
            DecalStateManager.RedoLatestUndo();
        }

        private void OnUndoStackChange()
        {
            undoButton.interactable = DecalStateManager.CanUndo;
        }
        
        private void OnRedoStackChange()
        {
            redoButton.interactable = DecalStateManager.CanRedo;
        }
        
        private void OnSelectDecal(LiveDecal liveDecal)
        {
            trashButton.interactable = true;
        }
        
        private void OnDeselectDecal(LiveDecal liveDecal)
        {
            trashButton.interactable = false;
        }
        
    }
}
