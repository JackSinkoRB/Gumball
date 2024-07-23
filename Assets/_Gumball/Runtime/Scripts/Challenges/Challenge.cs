using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class Challenge
    {

        [SerializeField] private string description = "Description of challenge";
        
        public string Description => description;

    }
}
