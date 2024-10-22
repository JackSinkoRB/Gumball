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

        [SerializeField, DisplayInspector] private DialogueCharacter character;
        [SerializeField] private string message;

        public string Message => message;
        public DialogueCharacter Character => character;
        
    }
}
