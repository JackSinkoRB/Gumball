using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Dialogue Character")]
    public class DialogueCharacter : ScriptableObject
    {

        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;

        public string DisplayName => displayName;
        public Sprite Icon => icon;

    }
}
