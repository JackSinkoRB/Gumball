using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct Dialogue
    {
    
        public enum ScreenPosition
        {
            RIGHT,
            LEFT
        }

        [SerializeField, DisplayInspector] private DialogueCharacter character;
        [SerializeField] private ScreenPosition characterPosition;
        [SerializeField] private string message;

        public DialogueCharacter Character => character;
        public ScreenPosition CharacterPosition => characterPosition;
        public string Message => message;

    }
}
