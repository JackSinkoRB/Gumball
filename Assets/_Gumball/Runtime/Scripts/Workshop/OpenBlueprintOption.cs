using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class OpenBlueprintOption : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image icon;

        public void Initialise(Sprite iconSprite, string text)
        {
            icon.sprite = iconSprite;
            label.text = text;
        }

    }
}
