using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class TextureOptionUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;

        public Button Button => button;
        public Image Icon => icon;
    }
}
