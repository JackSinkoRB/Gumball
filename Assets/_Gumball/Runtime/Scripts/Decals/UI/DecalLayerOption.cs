using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(MultiImageButton))]
    public class DecalLayerOption : MonoBehaviour
    {

        [SerializeField] private Image textureIcon;
        [SerializeField] private TextMeshProUGUI priorityLabel;

        private LiveDecal liveDecal;
        
        public Image TextureIcon => textureIcon;
        
        public void Initialise(LiveDecal liveDecal)
        {
            this.liveDecal = liveDecal;
            
            textureIcon.sprite = liveDecal.Sprite;
            textureIcon.color = liveDecal.Color;
            priorityLabel.text = liveDecal.Priority.ToString();
        }
        
        public void OnClickButton()
        {
            DecalEditor.Instance.SelectLiveDecal(liveDecal);
        }
        
    }
}
