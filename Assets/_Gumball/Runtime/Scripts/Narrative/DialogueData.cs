
using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class DialogueData
    {

        [SerializeField] private Dialogue[] dialogue = Array.Empty<Dialogue>();

        [SerializeField, ReadOnly] private string uniqueID = Guid.NewGuid().ToString();
        
        public bool HasBeenCompleted
        {
            get => DataManager.Dialogue.Get($"HasBeenCompleted.{uniqueID}", false);
            private set => DataManager.Dialogue.Set($"HasBeenCompleted.{uniqueID}", value);
        }
        
        public Dialogue[] Dialogue => dialogue;
        
        public void Play()
        {
            DialogueManager.Play(this);
        }

        public void OnComplete()
        {
            HasBeenCompleted = true;
        }
        
    }
}
