using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(Button))]
    public class WheelModificationPositionButton : MonoBehaviour
    {

        [SerializeField] private Color selectedColor;
        [SerializeField] private Color deselectedColor;
        [SerializeField] private float selectionTweenDuration = 0.3f;

        private bool isSelected;
        private Tween selectionTween;
        
        public Button Button => GetComponent<Button>();
        
        public void Select()
        {
            isSelected = true;
            
            selectionTween?.Kill();
            selectionTween = Button.image.DOColor(selectedColor, selectionTweenDuration);
        }

        public void Deselect()
        {
            isSelected = false;
            
            selectionTween?.Kill();
            selectionTween = Button.image.DOColor(deselectedColor, selectionTweenDuration);
        }
        
    }
}
