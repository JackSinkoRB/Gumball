using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(MatchSizeOfRects))]
    public class ChallengeRewardUI : MonoBehaviour
    {

        [SerializeField] private Image icon;
        [SerializeField] private AutosizeTextMeshPro label;

        private MatchSizeOfRects matchSizeOfRects => GetComponent<MatchSizeOfRects>();

        public void Initialise(Sprite sprite, string text)
        {
            icon.sprite = sprite;
            label.text = text;
            label.Resize();
            
            matchSizeOfRects.Resize();
        }
        
    }
}
