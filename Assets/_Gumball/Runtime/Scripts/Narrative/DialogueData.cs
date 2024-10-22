
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class DialogueData
    {

        [SerializeField] private Dialogue[] dialogue = Array.Empty<Dialogue>();

        public Dialogue[] Dialogue => dialogue;
        
        public void Play()
        {
            DialogueManager.Play(this);
        }
        
    }
}
