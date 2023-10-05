using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class TextureOptionUI : MonoBehaviour
    {

        [SerializeField] private Image textureImage;

        public Image TextureImage => textureImage;

        public void OnClick()
        {
            LiveDecal decal = DecalManager.Instance.CreateLiveDecal(textureImage.sprite.texture);
            DecalManager.Instance.SelectLiveDecal(decal);
        }
        
    }
}
