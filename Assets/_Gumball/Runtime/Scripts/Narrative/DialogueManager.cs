using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class DialogueManager
    {
        
        private static DialogueData currentData;
        private static int currentIndex;

        public static bool IsPlaying => currentData != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            currentData = null;
        }
        
        public static void Play(DialogueData data)
        {
            if (data.Dialogue.Length == 0)
            {
                Debug.LogError("Dialogue data doesn't contain any dialogue.");
                return;
            }
            
            if (currentData != null)
                Debug.LogWarning("Overriding previous dialogue that was playing.");

            currentData = data;
            ShowDialogue();
        }

        private static void ShowDialogue()
        {
            ShowDialogue(0);

            PrimaryContactInput.onPress += OnPrimaryContactPress;
        }

        private static void OnDialogueComplete()
        {
            currentData.OnComplete();
            currentData = null;
            
            PrimaryContactInput.onPress -= OnPrimaryContactPress;

            PanelManager.GetPanel<DialoguePanel>().Hide();
        }

        private static void OnPrimaryContactPress()
        {
            CheckToShowNextDialogue();
        }

        private static void CheckToShowNextDialogue()
        {
            int newIndex = currentIndex + 1;
            if (newIndex >= currentData.Dialogue.Length)
            {
                OnDialogueComplete();
                return;
            }
            
            ShowDialogue(newIndex);
        }

        private static void ShowDialogue(int index)
        {
            if (index < 0 || index >= currentData.Dialogue.Length)
                throw new IndexOutOfRangeException($"Index {index} is out of bounds for the current dialogue data.");

            currentIndex = index;
            
            Dialogue dialogue = currentData.Dialogue[index];
            PanelManager.GetPanel<DialoguePanel>().Initialise(dialogue);
            PanelManager.GetPanel<DialoguePanel>().Show();
        }
        
    }
}
