using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class GameSessionNodeReward : MonoBehaviour
    {

        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI label;

        public void Initialise(string labelText, Sprite icon)
        {
            label.text = labelText;
            image.sprite = icon;
        }

    }
}
