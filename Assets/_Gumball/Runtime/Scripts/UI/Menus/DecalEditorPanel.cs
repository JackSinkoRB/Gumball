using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalEditorPanel : AnimatedPanel
    {

        [Header("Decal editor panel")]
        [SerializeField] private Button trashButton;
        [SerializeField] private Button colourButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        
        [Header("Colour picker")]
        [SerializeField] private DecalColourSelectorPanel colourSelectorPanel;
        [SerializeField] private Image colourPickerGlow;
        [SerializeField] private float colourPickerGlowDuration = 0.1f;
        
        public Button TrashButton => trashButton;
        public Button ColourButton => colourButton;

        public DecalColourSelectorPanel ColourSelectorPanel => colourSelectorPanel;

        private bool colourPickerEnabled;
        private Tween colourPickerGlowTween;
        
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
            
            //nothing will be selected, so disable the buttons
            trashButton.interactable = false;
            colourButton.interactable = false;

            DisableColourPicker();
            colourPickerGlowTween?.Complete();
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

        public void OnClickColourButton()
        {
            if (colourPickerEnabled)
                DisableColourPicker();
            else
            {
                EnableColourPicker();
                ColourSelectorPanel.Populate(DecalEditor.Instance.CurrentSelected);
            }
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

            if (liveDecal.TextureData.CanColour)
            {
                colourButton.interactable = true;
                if (colourPickerEnabled)
                {
                    ColourSelectorPanel.Show();
                    ColourSelectorPanel.Populate(liveDecal);
                    ShowColourPickerOutline(true);
                }
            }
        }
        
        private void OnDeselectDecal(LiveDecal liveDecal)
        {
            trashButton.interactable = false;
            
            colourButton.interactable = false;
            ColourSelectorPanel.Hide();
            ShowColourPickerOutline(false);
        }

        private void EnableColourPicker()
        {
            colourPickerEnabled = true;
            colourSelectorPanel.Show();

            ShowColourPickerOutline(true);
        }

        private void DisableColourPicker()
        {
            colourPickerEnabled = false;
            colourSelectorPanel.Hide();

            ShowColourPickerOutline(false);
        }

        private void ShowColourPickerOutline(bool show)
        {
            colourPickerGlowTween?.Kill();
            colourPickerGlowTween = colourPickerGlow.DOFade(show ? 1 : 0, colourPickerGlowDuration);
        }
        
    }
}
