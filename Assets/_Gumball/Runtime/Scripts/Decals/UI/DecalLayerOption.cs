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
        [SerializeField] private FrameUI frame;
        
        private LiveDecal liveDecal;
        
        public Image TextureIcon => textureIcon;
        
        public void Initialise(LiveDecal liveDecal)
        {
            this.liveDecal = liveDecal;
            
            textureIcon.sprite = liveDecal.Sprite;
            textureIcon.color = liveDecal.Color;
            priorityLabel.text = liveDecal.Priority.ToString();
            
            frame.gameObject.SetActive(false);
        }
        
        public void OnSelect()
        {
            DecalEditor.Instance.SelectLiveDecal(liveDecal);
            
            frame.gameObject.SetActive(true);

            DecalEditor.onDeselectLiveDecal += OnDeselect;
        }

        private void OnDeselect(LiveDecal liveDecal)
        {
            DecalEditor.onDeselectLiveDecal -= OnDeselect;
            
            frame.gameObject.SetActive(false);
        }
        
    }
}
