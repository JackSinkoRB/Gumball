using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ChallengeRewardUI : MonoBehaviour
    {

        [SerializeField] private Image icon;
        [SerializeField] private AutosizeTextMeshPro label;

        public void Initialise(Sprite sprite, string text)
        {
            icon.sprite = sprite;
            label.text = text;
            label.Resize();
        }
        
    }
}
