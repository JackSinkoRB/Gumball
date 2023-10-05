using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SelectedDecalUI : MonoBehaviour
    {

        [SerializeField] private Image invalidDecal;
        [Space(5)]
        [SerializeField] private Image image;
        [SerializeField] private Color validColor;
        [SerializeField] private Color invalidColor;

        public void Update()
        {
            LiveDecal selected = DecalManager.Instance.CurrentSelected;
            if (selected == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(selected.transform.position);
            transform.position = screenPos;

            image.color = selected.IsValidPosition ? validColor : invalidColor;
            
            invalidDecal.gameObject.SetActive(!selected.IsValidPosition);
            if (selected.IsValidPosition)
                invalidDecal.sprite = selected.Sprite;
        }
        
    }
}
