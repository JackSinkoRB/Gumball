using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalEditorPanel : AnimatedPanel
    {

        [Header("Decal editor panel")]
        [SerializeField] private DecalLayerSelector layerSelector;
        [SerializeField] private Button trashButton;
        [SerializeField] private Button colourButton;
        [SerializeField] private Button sendForwardButton;
        [SerializeField] private Button sendBackwardButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        
        [Header("Colour picker")]
        [SerializeField] private DecalColourSelectorPanel colourSelectorPanel;
        [SerializeField] private Image colourPickerGlow;
        [SerializeField] private float colourPickerGlowDuration = 0.1f;
        
        public Button TrashButton => trashButton;
        public Button ColourButton => colourButton;
        public Button SendForwardButton => sendForwardButton;
        public Button SendBackwardButton => sendBackwardButton;

        public DecalColourSelectorPanel ColourSelectorPanel => colourSelectorPanel;

        private bool colourPickerEnabled;
        private Tween colourPickerGlowTween;
        private List<LiveDecal> overlappingDecals;

        protected override void OnShow()
        {
            base.OnShow();
            
            DecalEditor.onSelectLiveDecal += OnSelectDecal;
            DecalEditor.onDeselectLiveDecal += OnDeselectDecal;
            DecalEditor.onCreateLiveDecal += OnCreateDecal;
            DecalEditor.onDestroyLiveDecal += OnDestroyDecal;
            
            DecalStateManager.onUndoStackChange += OnUndoStackChange;
            DecalStateManager.onRedoStackChange += OnRedoStackChange;

            //starting a new session, so the stacks will have changed
            OnUndoStackChange();
            OnRedoStackChange();
            
            //nothing will be selected, so disable the buttons
            trashButton.interactable = false;
            sendForwardButton.interactable = false;
            sendBackwardButton.interactable = false;
            colourButton.interactable = false;

            DisableColourPicker();
            colourPickerGlowTween?.Complete();
        }

        protected override void OnHide()
        {
            base.OnHide();

            DecalEditor.onSelectLiveDecal -= OnSelectDecal;
            DecalEditor.onDeselectLiveDecal -= OnDeselectDecal;
            DecalEditor.onCreateLiveDecal -= OnCreateDecal;
            DecalEditor.onDestroyLiveDecal -= OnDestroyDecal;
            
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

        public void OnClickSendForwardButton()
        {
            SendBackwardOrForward(true);
        }
        
        public void OnClickSendBackwardButton()
        {
            SendBackwardOrForward(false);
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
        
        private void OnCreateDecal(LiveDecal liveDecal)
        {
            UpdateSendForwardBackwardButtons();
        }
        
        private void OnDestroyDecal(LiveDecal liveDecal)
        {
            UpdateSendForwardBackwardButtons();
        }
        
        private void OnSelectDecal(LiveDecal liveDecal)
        {
            liveDecal.onMoved += OnSelectedDecalMoved;
            
            trashButton.interactable = true;
            
            UpdateSendForwardBackwardButtons();

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
        
        //TODO: write tests for decal priorities - the liveDecals list should always be in order of priority, and the decals priorities should go from 1 to X with no breaks in between

        private void OnDeselectDecal(LiveDecal liveDecal)
        {
            liveDecal.onMoved -= OnSelectedDecalMoved;

            trashButton.interactable = false;
            sendForwardButton.interactable = false;
            sendBackwardButton.interactable = false;
            
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
        
        private void OnSelectedDecalMoved()
        {
            UpdateSendForwardBackwardButtons();   
        }
        
        private void SendBackwardOrForward(bool isForward)
        {
            DecalEditor.Instance.CurrentSelected.SendBackwardOrForward(isForward, overlappingDecals);
            
            UpdateSendForwardBackwardButtons();
            
            layerSelector.PopulateScroll(); //order has changed, so need to repopulate
            layerSelector.SnapToLiveDecal(DecalEditor.Instance.CurrentSelected);
        }
        
        private void UpdateSendForwardBackwardButtons()
        {
            overlappingDecals = DecalEditor.Instance.CurrentSelected.GetOverlappingLiveDecals();

            bool hasDecalWithHigherPriority = false;
            bool hasDecalWithLowerPriority = false;

            foreach (LiveDecal decal in overlappingDecals)
            {
                if (hasDecalWithHigherPriority && hasDecalWithLowerPriority)
                    break; //no need to continue
                
                if (decal.Priority > DecalEditor.Instance.CurrentSelected.Priority)
                {
                    hasDecalWithHigherPriority = true;
                    continue;
                }
                
                if (decal.Priority < DecalEditor.Instance.CurrentSelected.Priority)
                {
                    hasDecalWithLowerPriority = true;
                    continue;
                }
            }

            sendForwardButton.interactable = hasDecalWithHigherPriority;
            sendBackwardButton.interactable = hasDecalWithLowerPriority;
        }
        
    }
}
