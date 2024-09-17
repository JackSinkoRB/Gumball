using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class RewardUI : MonoBehaviour
    {

        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI label;

        public void Initialise(Sprite icon, string text)
        {
            iconImage.sprite = icon;
            label.text = text;
        }

    }
}
