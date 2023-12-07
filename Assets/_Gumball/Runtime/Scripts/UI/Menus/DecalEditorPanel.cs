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
        
        public DecalLayerSelector LayerSelector => layerSelector;

        protected override void OnShow()
        {
            base.OnShow();
            
            DecalStateManager.onUndoStackChange += OnUndoStackChange;
            DecalStateManager.onRedoStackChange += OnRedoStackChange;

            //starting a new session, so the stacks will have changed
            OnUndoStackChange();
            OnRedoStackChange();
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            DecalStateManager.onUndoStackChange -= OnUndoStackChange;
            DecalStateManager.onRedoStackChange -= OnRedoStackChange;
        }

        public void OnClickBackButton()
        {
            DecalEditor.Instance.EndSession();
            MainSceneManager.LoadMainScene();
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
        
    }
}
